using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RedChess.WebEngine.Repositories.Interfaces;

namespace RedChess.WebEngine.Repositories
{
    internal class AnalysisRepository : IAnalysisRepository
    {
        public string AnalysisForGameMove(int gameId, int moveId)
        {
            using (var context = new ChessContext())
            {
                var entry = context.AnalysisEntries.FirstOrDefault(x => x.GameId == gameId && x.MoveNumber == moveId);
                return entry == null ? String.Empty : entry.Analysis;
            }
        }
    }
}
