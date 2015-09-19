using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Data.Entity.Migrations;
using System.Diagnostics;
using System.Linq;
using Microsoft.Azure;
using RedChess.WebEngine.Models;

namespace RedChess.WebEngine.Repositories
{
    internal class GameRepository : IGameRepository
    {
        public GameDto FindById(int id)
        {
            using (var context = new ChessContext())
            {
                var game = context.Boards.Include(b => b.UserProfileBlack).Include(b => b.UserProfileWhite).Include(b => b.UserProfileWinner).Single(b => b.GameId == id);
                return game;
            }
        }

        public void AddAnalysis(int id, int moveNumber, string analysisText)
        {
            var connectionString = CloudConfigurationManager.GetSetting("DefaultConnection");
            using (var context = new ChessContext(connectionString))
            {
                context.AnalysisEntries.Add(new AnalysisEntry()
                {
                    GameId = id,
                    MoveNumber = moveNumber,
                    Analysis = analysisText
                });
                context.SaveChanges();
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
                return context.Boards.Include(b => b.UserProfileBlack).Include(b => b.UserProfileWhite).Include(b => b.UserProfileWinner).ToList();
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
                        .ToList();
            }
        }
    }
}