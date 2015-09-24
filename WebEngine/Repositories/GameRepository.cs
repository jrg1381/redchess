using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Data.Entity.Infrastructure;
using System.Data.Entity.Migrations;
using System.Diagnostics;
using System.Linq;
using Microsoft.Azure;
using RedChess.WebEngine.Models;
using RedChess.WebEngine.Repositories.Interfaces;

namespace RedChess.WebEngine.Repositories
{
    internal class GameRepository : IGameRepository
    {
        public GameDto FindById(int id)
        {
            using (var context = new ChessContext())
            {
                var game = context.Boards
                    .Include(b => b.UserProfileBlack)
                    .Include(b => b.UserProfileWhite)
                    .Include(b => b.UserProfileWinner)
                    .AsNoTracking()
                    .Single(b => b.GameId == id);

                return game;
            }
        }

        public void AddAnalysis(int id, int moveNumber, string analysisText, int boardEvaluation)
        {
            var connectionString = CloudConfigurationManager.GetSetting("DefaultConnection");
            using (var context = new ChessContext(connectionString))
            {
                context.AnalysisEntries.Add(new AnalysisEntry()
                {
                    GameId = id,
                    MoveNumber = moveNumber,
                    Analysis = analysisText,
                    Evaluation = boardEvaluation
                });
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

        public IEnumerable<GameDto> FindAll()
        {
            using (var context = new ChessContext())
            {
                return context.Boards
                    .Include(b => b.UserProfileBlack)
                    .Include(b => b.UserProfileWhite)
                    .Include(b => b.UserProfileWinner)
                    .AsNoTracking()
                    .ToList();
            }
        }

        public IEnumerable<GameDto> FindWithPlayer(string userName)
        {
            using (var context = new ChessContext())
            {
                return
                    context.Boards.Include(b => b.UserProfileBlack)
                        .Include(b => b.UserProfileWhite)
                        .Include(b => b.UserProfileWinner)
                        .Where(b => b.UserProfileBlack.UserName == userName || b.UserProfileWhite.UserName == userName)
                        .AsNoTracking()
                        .ToList();
            }
        }
    }
}