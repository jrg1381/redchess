using System.Data.Entity.Migrations;
using System.Linq;
using RedChess.WebEngine.Models;

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

        public void TimeGameOut(int id, string message, string userName)
        {
            using (var context = new ChessContext())
            {
                var gameRepository = new GameRepository();
                var game = gameRepository.FindById(id);

                // Because the user who timed out hasn't made a move to update the clock, the database will contains an elapsed time
                // which is too small. It will be equal to the time spent on all their _completed_ moves, and not the time they spent
                // thinking about the last one. This has the annoying effect that reloading the page will increase their time from 0.
                // To fix this, edit the loser's elapsed time in the database and set it equal to the time limit for the game.

                var timeLimit = Clock(id).TimeLimitMs;

                if (userName == game.UserProfileWhite.UserName)
                {
                    context.Clocks.Single(clock => clock.GameId == id).TimeElapsedWhiteMs = timeLimit;
                }
                if (userName == game.UserProfileBlack.UserName)
                {
                    context.Clocks.Single(clock => clock.GameId == id).TimeElapsedBlackMs = timeLimit;
                }

                new GameManager().EndGameWithMessage(id, message);
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