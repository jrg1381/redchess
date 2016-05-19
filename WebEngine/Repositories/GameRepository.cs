using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Data.Entity.Infrastructure;
using System.Data.Entity.Migrations;
using System.Linq;
using LinqToQuerystring;
using Microsoft.Azure;
using RedChess.ChessCommon;
using RedChess.ChessCommon.Enumerations;
using RedChess.ChessCommon.Interfaces;
using RedChess.WebEngine.Models;
using RedChess.WebEngine.Repositories.Interfaces;

namespace RedChess.WebEngine.Repositories
{
    internal class GameRepository : IGameRepository
    {
        public string Fen(int gameId)
        {
            using (var context = new ChessContext())
            {
                var game = context.Boards
                    .AsNoTracking()
                    .Where(b => b.GameId == gameId)
                    .Select(g => g.Fen)
                    .Single();

                return game;
            }
        }

        public object FindWhere(string queryString)
        {
            using (var context = new ChessContext())
            {
                var game = context.Boards
                    .Include(b => b.UserProfileBlack)
                    .Include(b => b.UserProfileWhite)
                    .Include(b => b.UserProfileWinner)
                    .AsNoTracking();

                IEnumerable<object> queriedResult = (IEnumerable<object>) game.LinqToQuerystring(typeof(GameDto), queryString);
                return queriedResult.ToArray();
            }
        }

        public GameDto RecordMove(int gameId, string fen, string lastMove, DateTime moveReceivedAt, GameStatus status, int? winnerUserId, Location start, Location end)
        {
            using (var context = new ChessContext())
            {
                var query = context.Database.SqlQuery<GameDto>( 
                    "EXEC dbo.RecordMove @p0, @p1, @p2, @p3, @p4, @p5, @p6, @p7, @p8", 
                    gameId, 
                    fen, 
                    lastMove,
                    moveReceivedAt,
                    status.FriendlyName(),
                    status.GameOver(),
                    winnerUserId,
                    start,
                    end);

                return query.First();
            }
        }

        public GameDto FindById(int id)
        {
            using (var context = new ChessContext())
            {
                var game = context.Boards
                    .Include(b => b.UserProfileBlack)
                    .Include(b => b.UserProfileWhite)
                    .Include(b => b.UserProfileWinner)
                    .Include(b => b.Clock)
                    .AsNoTracking()
                    .FirstOrDefault(b => b.GameId == id);

                return game;
            }
        }

        public void AddAnalysis(int id, int moveNumber, IProcessedAnalysis boardAnalysis)
        {
            var connectionString = CloudConfigurationManager.GetSetting("DefaultConnection");
            using (var context = new ChessContext(connectionString))
            {
                var analysisEntry = new AnalysisEntry()
                {
                    GameId = id,
                    MoveNumber = moveNumber,
                    Evaluation = boardAnalysis.BoardEvaluation,
                    EvaluationType = boardAnalysis.BoardEvaluationType,
                };

                context.AnalysisEntries.Add(analysisEntry);
                context.SaveChanges();

                var analysisLines = boardAnalysis.Analysis.Select(a => new AnalysisLine()
                {
                    GameId = id,
                    AnalysisEntryId = analysisEntry.AnalysisEntryId,
                    Fen = a.Fen,
                    Move = a.Move,
                    MoveNumber = a.MoveNumber++
                });

                context.AnalysisLines.AddRange(analysisLines);
                context.SaveChanges();
            }
        }

        public void Delete(int id)
        {
            using (var dbContext = new ChessContext())
            {
                // Crazy 'best' way to remove an object by id in EF without requesting the object first
                var game = new GameDto {GameId = id};
                dbContext.Boards.Attach(game);
                dbContext.Boards.Remove(game);
                try
                {
                    dbContext.SaveChanges();
                }
                catch (DbUpdateConcurrencyException)
                {
                    // Can throw if the object isn't there
                }
            }
        }

        public void AddOrUpdate(GameDto data)
        {
            using (var dbContext = new ChessContext())
            {
                dbContext.Boards.AddOrUpdate(data);
                dbContext.SaveChanges();
            }
        }
    }
}