using RedChess.WebEngine.Models;

namespace RedChess.WebEngine.Repositories
{
    internal interface IClockRepository
    {
        IClock Clock(int gameId);
        void SaveClock(IClock clock);
        void AddClock(int gameId, int timeLimitMs);
    }
}