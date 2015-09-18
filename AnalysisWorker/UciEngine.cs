using System;
using System.ComponentModel.Design;

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
            m_engine.WaitForReady();
            var options = m_engine.Write("uci", "uciok");
            m_engine.Write("isready", "readyok");
            SetOptions();
            NewGame();
        }

        internal void NewGame()
        {
            m_engine.Write("ucinewgame");
        }

        internal void SetOptions()
        {
            m_engine.Write("set option name Hash 32");
        }

        internal string BestMove(string fen)
        {
            var cmd = String.Format("position fen {0}\r\ngo", fen);
            var analysis = m_engine.Write(cmd, "bestmove");
            return analysis.Substring(analysis.LastIndexOf("bestmove", StringComparison.Ordinal) + 9, 4);
        }

        public void Dispose()
        {
            m_engine.Close();
        }
    }
}
