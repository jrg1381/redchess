using System;
using System.Collections;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RedChess.ChessCommon.Interfaces;
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
                        context.AnalysisEntries
                        .Where(e => e.GameId == oldGameId && e.MoveNumber <= cloneUpToMove))
                {
                    var newEntry = new AnalysisEntry()
                    {
                        MoveNumber = entry.MoveNumber,
                        GameId = newGameId,
                        Evaluation = entry.Evaluation,
                        AnalysisLines = entry.AnalysisLines.ToList()
                    };
                    context.AnalysisEntries.Add(newEntry);
                }

                context.SaveChanges();
            }
        }

        public IEnumerable<IAnalysisBinding> AnalysisForGameMoves(int gameId)
        {
            using (var context = new ChessContext())
            {
                var entries = context.AnalysisEntries
                    .AsNoTracking()
                    .Where(
                        x => x.GameId == gameId)
                    .Include(b => b.AnalysisLines)
                    .ToList();

                return entries.Select(x => new AnalysisBinding()
                {
                    AnalysisLines = x.AnalysisLines
                    .Select(y => y as IHistoryEntry)
                    .OrderBy(h => h.MoveNumber)
                    .ToList(),
                    Evaluation = x.Evaluation,
                    BoardEvaluationType = x.EvaluationType,
                    MoveNumber = x.MoveNumber
                });
            }
        }
    }
}
