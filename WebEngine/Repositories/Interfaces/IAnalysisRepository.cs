namespace RedChess.WebEngine.Repositories.Interfaces
{
    internal interface IAnalysisRepository
    {
        IAnalysisBinding AnalysisForGameMove(int gameId, int moveId);
    }
}