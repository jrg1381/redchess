using System.Data.Entity.Migrations;
using System.Linq;
using RedChess.WebEngine.Models;
using RedChess.WebEngine.Repositories.Interfaces;

namespace RedChess.WebEngine.Repositories
{
    internal class ClockRepository : IClockRepository
    {
        public IClock Clock(int gameId)
        {
            using (var dbContext = new ChessContext())
            {
                return dbContext.Clocks.FirstOrDefault(clock => clock.GameId == gameId);
            }
        }

        public void SaveClock(IClock clock)
        {
            using (var context = new ChessContext())
            {
                context.Clocks.AddOrUpdate(clock as Clock);
                context.SaveChanges();
            }
        }

        public void AddClock(int gameId, int timeLimitMs)
        {
            using (var context = new ChessContext())
            {
                var clock = new Clock(gameId, timeLimitMs);
                context.Clocks.Add(clock);
                context.SaveChanges();
            }
        }
    }
}