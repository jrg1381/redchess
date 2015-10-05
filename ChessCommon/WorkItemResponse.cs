using RedChess.ChessCommon.Enumerations;
using RedChess.ChessCommon.Interfaces;

namespace RedChess.ChessCommon
{
    public class WorkItemResponse : IWorkItemResponse
    {
        public WorkItemResponse(IWorkItemResponse bestMove)
        {
            Analysis = bestMove.Analysis;
            BoardEvaluation = bestMove.BoardEvaluation;
            BoardEvaluationType = bestMove.BoardEvaluationType;
        }

        public WorkItemResponse()
        {
        }

        public string Analysis { get; set; }
        public int BoardEvaluation { get; set; }
        public EvaluationType BoardEvaluationType { get; set; }
    }
}