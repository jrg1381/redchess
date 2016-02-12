using RedChess.WebEngine.Models;

namespace RedChess.WebEngine.Repositories.Interfaces
{
    internal interface IClockRepository
    {
        IClock Clock(int gameId);
        void SaveClock(IClock clock);
        int AddClock(int gameId, int timeLimitMs);
        PlayerReadyStatus PlayerReady(int gameId, string playerColor);
    }
}