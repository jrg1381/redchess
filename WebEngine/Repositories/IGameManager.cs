using System.Collections.Generic;
using RedChess.ChessCommon.Interfaces;
using RedChess.WebEngine.Models;

namespace RedChess.WebEngine.Repositories
{
    public interface IGameManager
    {
        IGameBinding FetchGame(int gameId);
        int Add(IBoard board, int opponentId, string currentUser, bool playAsBlack, int timeLimitMs);
        void SaveClock(IClock clock);
        IClock Clock(int gameId);
        HistoryEntry FindByGameIdAndMoveNumber(int gameId, int moveNumber);
        IEnumerable<HistoryEntry> FindAllMoves(int game);
    }
}