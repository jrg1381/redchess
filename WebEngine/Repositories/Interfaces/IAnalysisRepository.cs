using System.Collections.Generic;

namespace RedChess.WebEngine.Repositories.Interfaces
{
    internal interface IAnalysisRepository
    {
        void CloneGame(int newGameId, int oldGameId, int cloneUpToMove);
        IEnumerable<IAnalysisBinding> AnalysisForGameMoves(int gameId);
    }
}