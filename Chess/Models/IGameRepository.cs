using System.Collections.Generic;

namespace Chess.Models
{
    public interface IGameRepository
    {
        IGame FindById(int id);
        void Delete(int id);
        void TimeGameOut(int id, string message, string userName);
        IEnumerable<IGame> FindAll();
        IGame Add(BoardImpl board, int opponentId, string user, bool playAsBlack);
    }
}