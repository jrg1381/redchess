using System.Collections.Generic;
using Chess.Models;

namespace Chess.Repositories
{
    public interface IHistoryRepository
    {
        HistoryEntry FindByGameIdAndMoveNumber(int gameId, int moveNumber);
        IEnumerable<HistoryEntry> FindAllMoves(int gameId);
        int LatestMoveInGame(int gameId);
        bool IsParticipant(string username, int gameId);
        void Add(HistoryEntry historyEntry);
    }
}