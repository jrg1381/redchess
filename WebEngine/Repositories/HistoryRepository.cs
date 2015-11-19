using System.Collections.Generic;
using System.Data.Entity.Migrations;
using System.Linq;
using Microsoft.Azure;
using RedChess.WebEngine.Models;
using RedChess.WebEngine.Repositories.Interfaces;

namespace RedChess.WebEngine.Repositories
{
    internal class HistoryRepository : IHistoryRepository
    {
        ///  Only used by the AnalysisWorker role
        public HistoryEntry FindByGameIdAndMoveNumber(int gameId, int moveNumber)
        {
            var connectionString = CloudConfigurationManager.GetSetting("DefaultConnection");
            using (var context = new ChessContext(connectionString))
            {
                return context.HistoryEntries
                    .AsNoTracking()
                    .FirstOrDefault(x => x.GameId == gameId && x.MoveNumber == moveNumber);
            }
        }

        public IEnumerable<HistoryEntry> FindAllMoves(int gameId)
        {
            using (var context = new ChessContext())
            {
                return context.HistoryEntries
                    .AsNoTracking()
                    .Where(x => x.GameId == gameId).OrderBy(x => x.MoveNumber).ToList();
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

        public void CloneGame(int newGameId, int oldGameId, int cloneUpToMove)
        {
            using (var context = new ChessContext())
            {
                foreach (
                    var entry in
                        context.HistoryEntries.Where(h => h.GameId == oldGameId && h.MoveNumber <= cloneUpToMove))
                {
                    var newEntry = new HistoryEntry()
                    {
                        MoveNumber = entry.MoveNumber,
                        Fen = entry.Fen,
                        Move = entry.Move,
                        GameId = newGameId
                    };
                    context.HistoryEntries.Add(newEntry);
                }

                context.SaveChanges();
            }
        }
    }
}