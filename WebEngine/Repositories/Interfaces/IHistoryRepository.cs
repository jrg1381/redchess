using System.Collections.Generic;
using RedChess.WebEngine.Models;

namespace RedChess.WebEngine.Repositories.Interfaces
{
    internal interface IHistoryRepository
    {
        HistoryEntry FindByGameIdAndMoveNumber(int gameId, int moveNumber);
        IEnumerable<HistoryEntry> FindAllMoves(int gameId);
        void Add(HistoryEntry historyEntry);
        void CloneGame(int newGameId, int oldGameId, int cloneUpToMove);
    }
}