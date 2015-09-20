using System;
using System.ComponentModel.Design;
using System.Diagnostics;

namespace Redchess.AnalysisWorker
{
    internal class UciEngine : IDisposable
    {
        private const string c_processReadyText = "Stockfish 6 64";
        private readonly BidirectionalProcess m_engine;

        internal UciEngine()
        {
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

        internal string BestMove(string fen)
        {
            Trace.WriteLine("Bestmove on "+ fen);
            var cmd = String.Format("position fen {0}\r\ngo movetime 5000", fen);
            var analysis = m_engine.Write(cmd, "ponder");
            return analysis;
            // return analysis.Substring(analysis.LastIndexOf("bestmove", StringComparison.Ordinal) + 9, 4);
        }

        public void Dispose()
        {
            m_engine.Close();
        }
    }
}
