using System;
using System.ComponentModel.Design;
using System.Diagnostics;
using System.Text.RegularExpressions;

namespace Redchess.AnalysisWorker
{
    internal class UciEngine : IDisposable
    {
        private const string c_processReadyText = "Stockfish 6 64";
        private readonly BidirectionalProcess m_engine;
        private static Regex s_centipawnScoreRegex;

        internal UciEngine()
        {
            s_centipawnScoreRegex = new Regex(@"score cp (-?\d+)");
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

        internal WorkItemResponse Evaluate(WorkItem workItem)
        {
            return new WorkItemResponse
            {
                Analysis = BestMove(workItem),
                BoardEvaluation = ScoreForGivenMove(workItem)
            };
        }

        internal string BestMove(WorkItem workItem)
        {
            Trace.WriteLine("Bestmove on "+ workItem.Fen);
            var cmd = String.Format("position fen {0}\r\ngo movetime 5000", workItem.Fen);
            var analysis = m_engine.Write(cmd, "ponder");
            return analysis;
            return analysis.Substring(analysis.LastIndexOf("bestmove", StringComparison.Ordinal) + 9, 5).TrimEnd(new []{' '});
        }

        internal int ScoreForGivenMove(WorkItem workItem)
        {
            Trace.WriteLine("ScoreForGivenMove on " + workItem.Fen + " " + workItem.Move);
            var cmd = String.Format("position fen {0}\r\ngo searchmoves {1} movetime 5000", workItem.Fen, workItem.Move);
            var analysis = m_engine.Write(cmd, "ponder");
            var matches = s_centipawnScoreRegex.Match(analysis);
            
            if (matches.Success && matches.Captures.Count == 1)
            {
                Trace.WriteLine("Score for move is " + matches.Captures[0].Value);
                return Int32.Parse(matches.Captures[0].Value);
            }

            return Int32.MaxValue;
        }

        public void Dispose()
        {
            m_engine.Close();
        }
    }
}
