using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RedChess.WebEngine.Models;
using RedChess.WebEngine.Repositories.Interfaces;

namespace RedChess.WebEngine.Repositories
{
    internal class AnalysisRepository : IAnalysisRepository
    {
        public void CloneGame(int newGameId, int oldGameId, int cloneUpToMove)
        {
            using (var context = new ChessContext())
            {
                foreach (
                    var entry in
                        context.AnalysisEntries.Where(e => e.GameId == oldGameId && e.MoveNumber <= cloneUpToMove))
                {
                    var newEntry = new AnalysisEntry()
                    {
                        MoveNumber = entry.MoveNumber,
                        GameId = newGameId,
                        Evaluation = entry.Evaluation,
                        Analysis = entry.Analysis
                    };
                    context.AnalysisEntries.Add(newEntry);
                }

                context.SaveChanges();
            }
        }

        public IEnumerable<IAnalysisBinding> AnalysisForGameMoves(int gameId, int minMoveNumber, int maxMoveNumber)
        {
            using (var context = new ChessContext())
            {
                var entries = context.AnalysisEntries.Where(
                    x => x.GameId == gameId &&
                         x.MoveNumber >= minMoveNumber &&
                         x.MoveNumber <= maxMoveNumber).ToList();

                return entries.Select(x => new AnalysisBinding()
                {
                    AnalysisText = x.Analysis,
                    Evaluation = x.Evaluation,
                    BoardEvaluationType = x.EvaluationType,
                    MoveNumber = x.MoveNumber
                });
            }
        }
    }
}
