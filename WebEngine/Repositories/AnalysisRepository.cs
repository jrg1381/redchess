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
        public IAnalysisBinding AnalysisForGameMove(int gameId, int moveId)
        {
            using (var context = new ChessContext())
            {
                var entry = context.AnalysisEntries.FirstOrDefault(x => x.GameId == gameId && x.MoveNumber == moveId);
                return entry == null ? 
                    AnalysisBinding.Empty : 
                    new AnalysisBinding() {AnalysisText = entry.Analysis, Evaluation = entry.Evaluation};
            }
        }

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
    }
}
