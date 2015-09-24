using RedChess.WebEngine.Models;

namespace RedChess.WebEngine.Repositories.Interfaces
{
    internal interface IClockRepository
    {
        IClock Clock(int gameId);
        void SaveClock(IClock clock);
        void AddClock(int gameId, int timeLimitMs);
    }
}