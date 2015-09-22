using System.Collections.Generic;
using RedChess.WebEngine.Models;

namespace RedChess.WebEngine.Repositories
{
    internal interface IHistoryRepository
    {
        HistoryEntry FindByGameIdAndMoveNumber(int gameId, int moveNumber);
        IEnumerable<HistoryEntry> FindAllMoves(int gameId);
        bool IsParticipant(string username, int gameId);
        void Add(HistoryEntry historyEntry);
        void UpdateLastMove(HistoryEntry historyEntry);
        void CloneGame(int newGameId, int oldGameId, int cloneUpToMove);
    }
}