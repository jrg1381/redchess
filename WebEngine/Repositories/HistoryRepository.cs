using System.Collections.Generic;
using System.Data.Entity.Migrations;
using System.Linq;
using RedChess.WebEngine.Models;

namespace RedChess.WebEngine.Repositories
{
    internal class HistoryRepository : IHistoryRepository
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
                return context.HistoryEntries.Where(x => x.GameId == gameId).OrderBy(x => x.MoveNumber).ToList();
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

        public void UpdateLastMove(HistoryEntry historyEntry)
        {
            using (var context = new ChessContext())
            {
                var entryForLastMove = context.HistoryEntries.Where(x => x.GameId == historyEntry.GameId).OrderByDescending(x => x.MoveNumber).Take(1).Single();

                entryForLastMove.Fen = historyEntry.Fen;
                entryForLastMove.Move = historyEntry.Move;

                context.HistoryEntries.AddOrUpdate(entryForLastMove);
                context.SaveChanges();
            }
        }
    }
}