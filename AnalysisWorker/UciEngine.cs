using System;
using System.ComponentModel.Design;
using System.Diagnostics;
using System.Linq;

namespace Redchess.AnalysisWorker
{
    internal class UciEngine : IDisposable
    {
        private const string c_processReadyText = "Stockfish 6 64";
        private const string c_exePath = @"C:\Users\james.gilmore\Desktop\stockfish-6-win\Windows\stockfish-6-64.exe";
        private readonly BidirectionalProcess m_engine;

        internal UciEngine()
        {
            m_engine = new BidirectionalProcess(c_exePath, c_processReadyText);
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
