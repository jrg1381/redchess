using System;
using System.Data;
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

        public int AddClock(int gameId, int timeLimitMs)
        {
            using (var context = new ChessContext())
            {
                var clock = new Clock(gameId, timeLimitMs);
                context.Clocks.Add(clock);
                context.SaveChanges();
                return clock.ClockId;
            }
        }

        public PlayerReadyStatus PlayerReady(int gameId, string playerColor)
        {
            using (var context = new ChessContext())
            {
                RetryConfiguration.SuspendExecutionStrategy = true;
                Clock clock;

                using (var transaction = context.Database.BeginTransaction(IsolationLevel.Serializable))
                {
                    clock = context.Clocks.Single(c => c.GameId == gameId);

                    if (playerColor == "w")
                    {
                        clock.PlayersReady |= PlayerReadyStatus.White;
                    }
                    else if (playerColor == "b")
                    {
                        clock.PlayersReady |= PlayerReadyStatus.Black;
                    }

                    if (clock.PlayersReady == PlayerReadyStatus.Both)
                    {
                        clock.LastActionWhite = DateTime.UtcNow;
                    }

                    context.SaveChanges();
                    transaction.Commit();
                }

                RetryConfiguration.SuspendExecutionStrategy = false;
                return clock.PlayersReady;
            }
        }
    }
}