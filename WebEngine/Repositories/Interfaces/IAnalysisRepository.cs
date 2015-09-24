namespace RedChess.WebEngine.Repositories.Interfaces
{
    internal interface IAnalysisRepository
    {
        string AnalysisForGameMove(int gameId, int moveId);
    }
}