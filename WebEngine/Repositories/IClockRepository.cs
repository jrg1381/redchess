using RedChess.WebEngine.Models;

namespace RedChess.WebEngine.Repositories
{
    public interface IClockRepository
    {
        IClock Clock(int gameId);
        void SaveClock(IClock clock);
        void TimeGameOut(int id, string message, string userName);
        void AddClock(int gameId, int timeLimitMs);
    }
}