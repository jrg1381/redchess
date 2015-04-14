using System;
using System.Collections.Generic;
using System.Linq;
using Chess.Models;

namespace Chess.Repositories
{
    public class GameRepository : IGameRepository
    {
        public IGame FindById(int id)
        {
            using (var context = new ChessContext())
            {
                var game = context.Boards.Find(id);
                return new Game(game);
            }
        }

        public void Delete(int id)
        {
            using (var dbContext = new ChessContext())
            {
                var game = dbContext.Boards.Find(id);
                dbContext.Boards.Remove(game);
                dbContext.SaveChanges();
            }
        }

        public void TimeGameOut(int id, string message, string userName)
        {
            using (var context = new ChessContext())
            {
                var game = FindById(id);
                var clockRepository = new ClockRepository();

                // Because the user who timed out hasn't made a move to update the clock, the database will contains an elapsed time
                // which is too small. It will be equal to the time spent on all their _completed_ moves, and not the time they spent
                // thinking about the last one. This has the annoying effect that reloading the page will increase their time from 0.
                // To fix this, edit the loser's elapsed time in the database and set it equal to the time limit for the game.

                var timeLimit = clockRepository.Clock(id).TimeLimitMs;

                if (userName == game.UserProfileWhite.UserName)
                {
                    context.Clocks.Single(clock => clock.GameId == id).TimeElapsedWhiteMs = timeLimit;
                }
                if (userName == game.UserProfileBlack.UserName)
                {
                    context.Clocks.Single(clock => clock.GameId == id).TimeElapsedBlackMs = timeLimit;
                }

                game.EndGameWithMessage(id, message);
                context.SaveChanges();
            }
        }

        public IEnumerable<IGame> FindAll()
        {
            using (var context = new ChessContext())
            {
                return context.Boards.ToList().Select(x => new Game(x));
            }
        }

        public IGame Add(BoardImpl board, int opponentId, string user, bool playAsBlack)
        {
            using (var context = new ChessContext())
            {
                int userIdWhite, userIdBlack;

                if (playAsBlack)
                {
                    userIdWhite = opponentId;
                    userIdBlack = context.UserProfiles.Single(u => u.UserName == user).UserId;
                }
                else
                {
                    userIdBlack = opponentId;
                    userIdWhite = context.UserProfiles.Single(u => u.UserName == user).UserId; 
                }

                var game = new GameDto
                {
                    UserIdWhite = userIdWhite,
                    UserIdBlack = userIdBlack,
                    Status = String.Empty,
                    Fen = board.ToFen()
                };

                context.Boards.Add(game);
                context.SaveChanges();
                context.HistoryEntries.Add(new HistoryEntry() { GameId = game.GameId, Fen = game.Fen, MoveNumber = 1, Move = "" });
                context.SaveChanges();

                return new Game(game);
            }
        }
    }
}