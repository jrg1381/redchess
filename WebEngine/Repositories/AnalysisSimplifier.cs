using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text.RegularExpressions;
using RedChess.ChessCommon;
using RedChess.ChessCommon.Enumerations;
using RedChess.ChessCommon.Interfaces;
using RedChess.EngineFactory;
using RedChess.WebEngine.Models;
using RedChess.WebEngine.Repositories.Interfaces;

namespace RedChess.WebEngine.Repositories
{
    internal class AnalysisSimplifier
    {
        private readonly IHistoryRepository m_historyRepository;
        private static readonly Regex s_mateSequenceRegex, s_centipawnSequenceRegex;

        public AnalysisSimplifier(IHistoryRepository historyRepository)
        {
            m_historyRepository = historyRepository;
        }

        static AnalysisSimplifier()
        {
            var common = @"\s+nodes\s+\d+\s+nps\s+\d+\s+tbhits\s+\d+\s+time\s+\d+\s+pv\s+";
            s_mateSequenceRegex = new Regex(@"score\s+mate\s+-?\d+" +    common +  @"(([a-h][1-8][a-h][1-8][rqbn]?\s+)*)info\s+depth");
            s_centipawnSequenceRegex = new Regex(@"score\s+cp\s+-?\d+" + common + @"(([a-h][1-8][a-h][1-8][rqbn]?\s+)*)(info|bestmove)");
        }

        public IProcessedAnalysis ProcessBoardAnalysis(int gameId, int moveNumber, IBoardAnalysis inputAnalysis)
        {
            try
            {
                if (inputAnalysis.BoardEvaluationType == EvaluationType.MateInN)
                {
                    /* score mate -11 nodes 4884896 nps 1051419 tbhits 0 time 4646 pv 
                     * c4e2 d7b5 g1e1 b2d2 f2f1 d2f4 f1g2 b5e2 e1e2 f4f3 g2h2 f3e2 h2g3 
                     * e2f3 g3h2 e4e3 h2g1 e3e2 g1h2 e2e1n h2g1 f3g2 info depth 34 */
                    Trace.WriteLine("Mate in N detected");
                    return ProcessAnalysis(gameId, moveNumber, inputAnalysis, s_mateSequenceRegex);
                }
                if (inputAnalysis.BoardEvaluationType == EvaluationType.Centipawn)
                {
                    /* info depth 6 seldepth 6 multipv 1 score cp 21 nodes 4562 nps 350923 tbhits 0 time 13 pv g1f3 g8f6 d2d4 d7d5 b1c3 e7e6 */
                    Trace.WriteLine("Centipawn score detected");
                    return ProcessAnalysis(gameId, moveNumber, inputAnalysis, s_centipawnSequenceRegex);
                }
            }
            catch (Exception e)
            {
                Trace.WriteLine(inputAnalysis);
                Trace.TraceError("AnalysisSimplifier: " + e.Message);
                throw;
            }

            throw new ArgumentException("Could not process raw analysis");
        }

        private IProcessedAnalysis ProcessAnalysis(int gameId, int moveNumber, IBoardAnalysis inputAnalysis, Regex sequenceRegex)
        {
            var outputAnalysis = new ProcessedAnalysis(inputAnalysis);
            // Important - the board must be from the move BEFORE the analysed move
            var historyEntry = m_historyRepository.FindByGameIdAndMoveNumber(gameId, moveNumber);
            using (var board = BoardFactory.CreateInstance())
            {
                board.FromFen(historyEntry.Fen);
                var matches = sequenceRegex.Matches(inputAnalysis.Analysis);
                Trace.WriteLine(matches.Count + " regex matches detected");
                if(matches.Count == 0)
                    Trace.WriteLine("No match on : " + outputAnalysis.Analysis);
                if (matches.Count > 0)
                {
                    var lastMatch = matches[matches.Count - 1];
                    var winningMateSequence = lastMatch.Groups[1].Value.TrimEnd(' ');
                    var moves = winningMateSequence.Split(new[] {' '}, StringSplitOptions.RemoveEmptyEntries);

                    foreach (var move in moves)
                    {
                        var locations = MovesToLocations(new[] {move.Substring(0, 2), move.Substring(2, 2)});
                        if (!board.Move(locations[0], locations[1]))
                            throw new ArgumentException($"Could not move from {locations[0]} to {locations[1]} on {board.ToFen()}");
                        if (move.Length == 5)
                        {
                            board.PromotePiece(move[4].ToString().ToUpperInvariant());
                        }
                        var lastMove = board.LastMove();
                        outputAnalysis.Analysis.Add(new HistoryEntry()
                        {
                            Fen = board.ToFen(),
                            Move = lastMove,
                            MoveNumber = moveNumber++,
                            GameId = gameId
                        });
                    }
                }
            }
            return outputAnalysis;
        }

        private IList<Location> MovesToLocations(IEnumerable<string> locations)
        {
            return locations.Select(s => (Location)Enum.Parse(typeof(Location), s.ToUpperInvariant())).ToList();
        }

    }
}