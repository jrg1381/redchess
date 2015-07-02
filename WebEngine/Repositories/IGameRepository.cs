using System.Collections.Generic;

namespace RedChess.WebEngine.Repositories
{
    internal interface IGameRepository
    {
        GameDto FindById(int id);
        void Delete(int id);
        IEnumerable<GameDto> FindAll();
        void AddOrUpdate(GameDto data);
        IEnumerable<GameDto> FindWithPlayer(string userName);
    }
}