using System;
using System.Collections.Generic;
using System.Linq;
using Chess.Models;
using System.Data.Entity;
using System.Data.Entity.Migrations;

namespace Chess.Repositories
{
    public class GameRepository : IGameRepository
    {
        public GameDto FindById(int id)
        {
            using (var context = new ChessContext())
            {
                var game = context.Boards.Include(b => b.UserProfileBlack).Include(b => b.UserProfileWhite).Single(b => b.GameId == id);
                return game;
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
                return context.Boards.ToList();
            }
        }
    }
}