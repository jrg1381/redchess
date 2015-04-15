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

        public int LatestMoveInGame(int gameId)
        {
            using (var context = new ChessContext())
            {
                if (context.HistoryEntries.Any(x => x.GameId == gameId))
                {
                    return context.HistoryEntries.Where(x => x.GameId == gameId).Max(x => x.MoveNumber);
                }
                return 0;
            }
        }

        public bool IsParticipant(string username, int gameId)
        {
            using (var context = new ChessContext())
            {
                return context.Database.SqlQuery<bool>("SELECT dbo.IsParticipant(@p0,@p1)", gameId, username).FirstOrDefault();
            }
        }

        public void Add(HistoryEntry historyEntry)
        {
            using (var context = new ChessContext())
            {
                context.HistoryEntries.Add(historyEntry);
                context.SaveChanges();
            }
        }
    }
}