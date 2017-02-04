using System;
using System.Diagnostics;
using System.Linq;
using System.Text.RegularExpressions;
using RedChess.ChessCommon;
using RedChess.ChessCommon.Enumerations;

namespace Redchess.AnalysisWorker
{
    internal class UciEngine : IUciEngine
    {
        public const int NoScoreParsed = 0x0feefeef;
        public const int MaxAnalysisTimeSeconds = 5;
        private const string c_ProcessReadyText = "Stockfish 6 64";
        private readonly BidirectionalProcess m_Engine;
        private static readonly Regex CentipawnScoreRegex;
        private static readonly Regex MateInNMovesRegex;
        private static readonly Regex BestMoveRegex;

        public int GameId { get; }

        static UciEngine()
        {
            // Looks like info depth 7 seldepth 7 multipv 1 score cp 57 nodes 633
            CentipawnScoreRegex = new Regex(@"score cp (-?\d+)");
            // Looks like info depth 1 seldepth 2 multipv 1 score mate 1 nodes 32
            MateInNMovesRegex = new Regex(@"score mate (-?\d+)");
            // Looks like bestmove e4e5 or bestmove e7e8q
            BestMoveRegex = new Regex(@"bestmove ([a-h][1-8][a-h][1-8][rnqb]?)");
        }

        internal UciEngine(int gameId)
        {
            GameId = gameId;

            var exePath = EngineDownloader.DownloadEngine();
            m_Engine = new BidirectionalProcess(exePath, c_ProcessReadyText);
            Trace.WriteLine("Waiting for engine to be ready");
            m_Engine.WaitForReady();
            Trace.WriteLine("Putting engine in UCI mode");
            m_Engine.Write("uci", "uciok");
            Trace.WriteLine("Engine in UCI mode");
            Trace.WriteLine("Firing 'isready'");
            m_Engine.Write("isready", "readyok");
            Trace.WriteLine("readyok");
            SetOptions();
            NewGame();
            Trace.WriteLine("Started UciEngine");
        }

        private void NewGame()
        {
            Trace.WriteLine("ucinewgame");
            m_Engine.Write("ucinewgame");
            Trace.WriteLine("ucinewgame complete");
        }

        private void SetOptions()
        {
            Trace.WriteLine("Setting options");
            m_Engine.Write("setoption name Hash value 32");
            m_Engine.Write("setoption name Threads value 2");
            Trace.WriteLine("Setting options complete");
        }

        public void Evaluate(WorkItem workItem)
        {
            workItem.Result = new BoardAnalysis();
            BestMove(workItem);
            Score(workItem);

            // Split into two strings, take the first character of the 2nd string. This shows whose turn it is.
            if (workItem.Fen.Split(new[] {' '},2)[1][0] == 'b')
                workItem.Result.BoardEvaluation = -workItem.Result.BoardEvaluation; // The score is given from the engine's point of view,so this means black scores should be negated
        }

        private void BestMove(WorkItem workItem)
        {
            Trace.WriteLine("Bestmove on "+ workItem.Fen);
            var cmd = $"position fen {workItem.Fen} {workItem.Move}\r\ngo movetime {MaxAnalysisTimeSeconds * 1000}";
            var analysis = m_Engine.Write(cmd, "bestmove");
            workItem.Result.Analysis = analysis;
        }

        private void Score(WorkItem workItem)
        {
            var bestMoveMatch = BestMoveRegex.Match(workItem.Result.Analysis);
            if (!bestMoveMatch.Success)
            {
                throw new ArgumentException("Analysis data did not contain a bestmove");
            }
            var bestMove = bestMoveMatch.Groups[1].Value;
            var lastLine = workItem.Result.Analysis.Split(new[] { "\r\n" }, StringSplitOptions.None).Last(x => x.Contains("pv " + bestMove));

            var centipawnScoreMatch = CentipawnScoreRegex.Match(lastLine);

            //Discard newlines
            workItem.Result.Analysis = workItem.Result.Analysis.Replace("\r\n", " ");

            if (centipawnScoreMatch.Success)
            {
                Trace.WriteLine("Score for move is " + centipawnScoreMatch.Captures[0].Value);
                workItem.Result.BoardEvaluation = Int32.Parse(centipawnScoreMatch.Groups[1].Value);
                workItem.Result.BoardEvaluationType = EvaluationType.Centipawn;
                return;
            }

            var mateInN = MateInNMovesRegex.Match(lastLine);
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
            m_Engine.Close();
        }
    }
}
