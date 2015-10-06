using RedChess.ChessCommon.Enumerations;

namespace RedChess.WebEngine.Repositories
{
    public interface IAnalysisBinding
    {
        string AnalysisText { get; set; }
        int Evaluation { get; set; }
        EvaluationType BoardEvaluationType { get; set; }
        int MoveNumber { get; set; }
        bool IsEmpty();
    }
}