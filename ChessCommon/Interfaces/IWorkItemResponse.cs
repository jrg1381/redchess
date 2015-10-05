using RedChess.ChessCommon.Enumerations;

namespace RedChess.ChessCommon.Interfaces
{
    public interface IWorkItemResponse
    {
        string Analysis { get; set; }
        int BoardEvaluation { get; set; }
        EvaluationType BoardEvaluationType { get; set; }
    }
}