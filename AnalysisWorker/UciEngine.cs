using System;
using System.ComponentModel.Design;
using System.Diagnostics;
using System.Linq;
using System.Text.RegularExpressions;
using RedChess.ChessCommon.Enumerations;

namespace Redchess.AnalysisWorker
{
    internal class UciEngine : IDisposable
    {
        public const int NoScoreParsed = 0x0feefeef;
        private const string c_processReadyText = "Stockfish 6 64";
        private readonly BidirectionalProcess m_engine;
        private static readonly Regex s_centipawnScoreRegex;
        private static readonly Regex s_mateInNMovesRegex;
        private static readonly Regex s_bestMoveRegex;

        internal int GameId { get; private set; }

        static UciEngine()
        {
            // Looks like info depth 7 seldepth 7 multipv 1 score cp 57 nodes 633
            s_centipawnScoreRegex = new Regex(@"score cp (-?\d+)");
            // Looks like info depth 1 seldepth 2 multipv 1 score mate 1 nodes 32
            s_mateInNMovesRegex = new Regex(@"score mate (-?\d+)");
            // Looks like bestmove e4e5 or bestmove e7e8q
            s_bestMoveRegex = new Regex(@"bestmove ([a-h][1-8][a-h][1-8][rnqb]?)");
        }

        internal UciEngine(int gameId)
        {
            GameId = gameId;

            var exePath = EngineDownloader.DownloadEngine();
            m_engine = new BidirectionalProcess(exePath, c_processReadyText);
            Trace.WriteLine("Waiting for engine to be ready");
            m_engine.WaitForReady();
            Trace.WriteLine("Putting engine in UCI mode");
            var options = m_engine.Write("uci", "uciok");
            Trace.WriteLine("Engine in UCI mode");
            Trace.WriteLine("Firing 'isready'");
            m_engine.Write("isready", "readyok");
            Trace.WriteLine("readyok");
            SetOptions();
            NewGame();
            Trace.WriteLine("Started UciEngine");
        }

        internal void NewGame()
        {
            Trace.WriteLine("ucinewgame");
            m_engine.Write("ucinewgame");
            Trace.WriteLine("ucinewgame complete");
        }

        internal void SetOptions()
        {
            Trace.WriteLine("Setting options");
            m_engine.Write("setoption name Hash value 32");
            m_engine.Write("setoption name Threads value 2");
            Trace.WriteLine("Setting options complete");
        }

        internal void Evaluate(WorkItem workItem)
        {
            workItem.Result = new WorkItemResponse();
            BestMove(workItem);
            Score(workItem);

            // Split into two strings, take the first character of the 2nd string. This shows whose turn it is.
            if (workItem.Fen.Split(new[] {' '},2)[1][0] == 'b')
                workItem.Result.BoardEvaluation = -workItem.Result.BoardEvaluation; // The score is given from the engine's point of view,so this means black scores should be negated
        }

        private void BestMove(WorkItem workItem)
        {
            Trace.WriteLine("Bestmove on "+ workItem.Fen);
            var cmd = String.Format("position fen {0} {1}\r\ngo movetime 5000", workItem.Fen, workItem.Move);
            var analysis = m_engine.Write(cmd, "bestmove");
            workItem.Result.Analysis = analysis;
        }

        private void Score(WorkItem workItem)
        {
            var bestMoveMatch = s_bestMoveRegex.Match(workItem.Result.Analysis);
            if (!bestMoveMatch.Success)
            {
                throw new ArgumentException("Analysis data did not contain a bestmove");
            }
            var bestMove = bestMoveMatch.Groups[1].Value;
            var lastLine = workItem.Result.Analysis.Split(new[] { "\r\n" }, StringSplitOptions.None).Last(x => x.Contains("pv " + bestMove));

            var centipawnScoreMatch = s_centipawnScoreRegex.Match(lastLine);
            if (centipawnScoreMatch.Success)
            {
                Trace.WriteLine("Score for move is " + centipawnScoreMatch.Captures[0].Value);
                workItem.Result.BoardEvaluation = Int32.Parse(centipawnScoreMatch.Groups[1].Value);
                workItem.Result.BoardEvaluationType = EvaluationType.Centipawn;
                return;
            }

            var mateInN = s_mateInNMovesRegex.Match(lastLine);
            if (mateInN.Success)
            {
                Trace.WriteLine("Score for move is " + mateInN.Captures[0].Value);
                workItem.Result.BoardEvaluation = Int32.Parse(mateInN.Groups[1].Value);
                workItem.Result.BoardEvaluationType = EvaluationType.MateInN;
                return;
            }

            throw new ArgumentException("No score parsed");
        }

        public void Dispose()
        {
            m_engine.Close();
        }
    }
}
