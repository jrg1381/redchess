﻿using System;
using System.ComponentModel.Design;
using System.Diagnostics;
using System.Linq;
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
            var analysis = BestMove(workItem);

            return new WorkItemResponse
            {
                Analysis = analysis,
                BoardEvaluation = Score(analysis)
            };
        }

        internal string BestMove(WorkItem workItem)
        {
            Trace.WriteLine("Bestmove on "+ workItem.Fen);
            var cmd = String.Format("position fen {0} {1}\r\ngo movetime 5000", workItem.Fen, workItem.Move);
            var analysis = m_engine.Write(cmd, "ponder");
            return analysis;
        }

        private int Score(string analysis)
        {
            var bestMove = analysis.Substring(analysis.LastIndexOf("bestmove", StringComparison.Ordinal) + 9, 5).TrimEnd(new[] { ' ' });
            var lastLine = analysis.Split(new[] {"\r\n"},StringSplitOptions.None).Last(x => x.Contains("pv " + bestMove));
            var matches = s_centipawnScoreRegex.Match(lastLine);

            if (matches.Success && matches.Groups.Count == 2)
            {
                Trace.WriteLine("Score for move is " + matches.Captures[0].Value);
                return Int32.Parse(matches.Groups[1].Value);
            }

            return Int32.MaxValue;
        }

        public void Dispose()
        {
            m_engine.Close();
        }
    }
}
