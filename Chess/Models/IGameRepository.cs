using System.Collections.Generic;
using Chess.Repositories;

namespace Chess.Models
{
    public interface IGameRepository
    {
        GameDto FindById(int id);
        void Delete(int id);
        IEnumerable<GameDto> FindAll();
        void AddOrUpdate(GameDto data);
    }
}