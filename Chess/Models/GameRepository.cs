using System;
using System.Collections.Generic;
using System.Data.Entity.Migrations;
using System.Linq;
using System.Net.Mime;
using RedChess.ChessCommon.Interfaces;
using WebGrease.Css.Extensions;

namespace Chess.Models
{
    public class GameRepository
    {
        public bool MayManipulateBoard(int gameId, string userName)
        {
            using (var context = new ChessContext())
            {
                var game = context.Boards.Find(gameId);
                return (game.UserProfileBlack.UserName == userName || game.UserProfileWhite.UserName == userName);
            }
        }

        public IGame FindById(int id)
        {
            using (var context = new ChessContext())
            {
                var game = context.Boards.Find(id);
                return new Game(game);
            }
        }

        public IGame Add(IBoard board, int owner, int opponent)
        {
            using (var context = new ChessContext())
            {
                var game = new GameDto
                {
                    UserIdWhite = owner,
                    UserIdBlack = opponent,
                    Status = String.Empty,
                    Fen = board.ToFen()
                };

                context.Boards.Add(game);
                context.SaveChanges();
                context.HistoryEntries.Add(new HistoryEntry() {GameId = game.GameId, Fen = game.Fen, MoveNumber = 1, Move = ""});
                context.SaveChanges();

                return new Game(game);
            }
        }

        public Clock Clock(int gameId)
        {
            using (var dbContext = new ChessContext())
            {
                return dbContext.Clocks.FirstOrDefault(clock => clock.GameId == gameId);
            }
        }

        public void Delete(int id)
        {
            using (var dbContext = new ChessContext())
            {
                var game = dbContext.Boards.Find(id);
                dbContext.Boards.Remove(game);
            }
        }

        public void TimeGameOut(int id, string message, string userName)
        {
            using (var context = new ChessContext())
            {
                var game = context.Boards.Find(id);
                var foo = new Game(game);
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

                foo.EndGameWithMessage(id, message);
                context.SaveChanges();
            }
        }

        public void SaveClock(Clock clock)
        {
            using (var context = new ChessContext())
            {
                context.Clocks.AddOrUpdate(clock);
                context.SaveChanges();
            }
        }

        internal IEnumerable<IGame> FindAll()
        {
            using (var context = new ChessContext())
            {
                return context.Boards.ToList().Select(x => new Game(x));
            }
        }

        internal void AddClock(int gameId, int timeLimitMs)
        {
            using (var context = new ChessContext())
            {
                var clock = new Clock(gameId, timeLimitMs);
                context.Clocks.Add(clock);
                context.SaveChanges();
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

                if (playAsBlack)
                {
                    
                }

                context.Boards.Add(game);
                context.SaveChanges();
                context.HistoryEntries.Add(new HistoryEntry() { GameId = game.GameId, Fen = game.Fen, MoveNumber = 1, Move = "" });
                context.SaveChanges();

                return new Game(game);
            }
        }
    }
}