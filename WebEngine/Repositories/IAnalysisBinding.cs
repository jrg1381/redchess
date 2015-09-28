namespace RedChess.WebEngine.Repositories
{
    public interface IAnalysisBinding
    {
        string AnalysisText { get; set; }
        int Evaluation { get; set; }
        bool IsEmpty();
    }
}