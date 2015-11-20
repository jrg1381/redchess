using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text.RegularExpressions;
using RedChess.ChessCommon;
using RedChess.ChessCommon.Enumerations;
using RedChess.ChessCommon.Interfaces;
using RedChess.EngineFactory;
using RedChess.WebEngine.Repositories.Interfaces;

namespace RedChess.WebEngine.Repositories
{
    internal class AnalysisSimplifier
    {
        private readonly IHistoryRepository m_historyRepository;
        private static readonly Regex s_mateSequenceRegex;

        public AnalysisSimplifier(IHistoryRepository historyRepository)
        {
            m_historyRepository = historyRepository;
        }

        static AnalysisSimplifier()
        {
            s_mateSequenceRegex = new Regex(@"score mate -?\d+ nodes \d+ nps \d+ tbhits \d+ time \d+ pv (.*?) info depth");
        }

        public IBoardAnalysis ProcessBoardAnalysis(int gameId, int moveNumber, IBoardAnalysis inputAnalysis)
        {
            try
            {
                   /* score mate -11 nodes 4884896 nps 1051419 tbhits 0 time 4646 pv 
                 * c4e2 d7b5 g1e1 b2d2 f2f1 d2f4 f1g2 b5e2 e1e2 f4f3 g2h2 f3e2 h2g3 
                 * e2f3 g3h2 e4e3 h2g1 e3e2 g1h2 e2e1n h2g1 f3g2 info depth 34 */
                if (inputAnalysis.BoardEvaluationType == EvaluationType.MateInN)
                {
                    Trace.WriteLine("Mate in N detected");
                    var outputAnalysis = new BoardAnalysis(inputAnalysis);
                    // Important - the board must be from the move BEFORE the analysed move, so -1
                    var historyEntry = m_historyRepository.FindByGameIdAndMoveNumber(gameId, moveNumber - 1);
                    using (var board = BoardFactory.CreateInstance())
                    {
                        board.FromFen(historyEntry.Fen);
                        var matches = s_mateSequenceRegex.Matches(outputAnalysis.Analysis);
                        Trace.WriteLine(matches.Count + " regex matches detected");
                        if (matches.Count > 0)
                        {
                            var lastMatch = matches[matches.Count - 1];
                            var matchBeginsAt = lastMatch.Groups[1].Index;
                            var moves = lastMatch.Groups[1].Value.Split(' ');
                            foreach (var move in moves)
                            {
                                var locations = MovesToLocations(new[] {move.Substring(0, 2), move.Substring(2, 2)});
                                if (!board.Move(locations[0], locations[1]))
                                    throw new ArgumentException(String.Format("Could not move from {0} to {1} on {2}",
                                        locations[0], locations[1], board.ToFen()));
                                var lastMove = board.LastMove();
                                outputAnalysis.Analysis =
                                    outputAnalysis.Analysis.Remove(matchBeginsAt, move.Length)
                                        .Insert(matchBeginsAt, lastMove);
                                matchBeginsAt += lastMove.Length + 1; // include the space
                            }
                        }
                    }
                    return outputAnalysis;
                }
            }
            catch (Exception e)
            {
                Trace.WriteLine(inputAnalysis);
                Trace.TraceError("AnalysisSimplifier: " + e.Message);
                throw;
            }

            return inputAnalysis;
        }

        private IList<Location> MovesToLocations(IEnumerable<string> locations)
        {
            return locations.Select(s => (Location)Enum.Parse(typeof(Location), s.ToUpperInvariant())).ToList();
        }

    }
}