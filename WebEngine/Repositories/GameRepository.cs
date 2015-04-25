using System.Collections.Generic;
using System.Data.Entity;
using System.Data.Entity.Migrations;
using System.Linq;

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
                return context.Boards.Include(b => b.UserProfileWinner).ToList();
            }
        }
    }
}