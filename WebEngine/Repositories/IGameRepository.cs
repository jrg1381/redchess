using System.Collections.Generic;

namespace RedChess.WebEngine.Repositories
{
    public interface IGameRepository
    {
        GameDto FindById(int id);
        void Delete(int id);
        IEnumerable<GameDto> FindAll();
        void AddOrUpdate(GameDto data);
    }
}