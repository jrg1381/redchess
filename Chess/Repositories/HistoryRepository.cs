using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Chess.Models;
using WebGrease.Css.Extensions;

namespace Chess.Repositories
{
    public class HistoryRepository
    {
        public HistoryEntry FindByGameIdAndMoveNumber(int gameId, int moveNumber)
        {
            using (var context = new ChessContext())
            {
                return context.HistoryEntries.FirstOrDefault(x => x.GameId == gameId && x.MoveNumber == moveNumber);
            }
        }

        public IEnumerable<HistoryEntry> FindAllMoves(int gameId)
        {
            using (var context = new ChessContext())
            {
                return context.HistoryEntries.Where(x => x.GameId == gameId).OrderBy(x => x.MoveNumber).ToSafeReadOnlyCollection();
            }
        }

        public bool IsParticipant(string username, int gameId)
        {
            using (var context = new ChessContext())
            {
                return context.Database.SqlQuery<bool>("SELECT dbo.IsParticipant(@p0,@p1)", gameId, username).FirstOrDefault();
            }
        }
    }
}