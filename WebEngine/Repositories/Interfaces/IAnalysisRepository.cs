namespace RedChess.WebEngine.Repositories.Interfaces
{
    internal interface IAnalysisRepository
    {
        IAnalysisBinding AnalysisForGameMove(int gameId, int moveId);
        void CloneGame(int newGameId, int oldGameId, int cloneUpToMove);
    }
}