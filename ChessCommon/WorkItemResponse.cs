using RedChess.ChessCommon.Enumerations;
using RedChess.ChessCommon.Interfaces;

namespace RedChess.ChessCommon
{
    public class BoardAnalysis : IBoardAnalysis
    {
        public BoardAnalysis(IBoardAnalysis bestMove)
        {
            Analysis = bestMove.Analysis;
            BoardEvaluation = bestMove.BoardEvaluation;
            BoardEvaluationType = bestMove.BoardEvaluationType;
        }

        public BoardAnalysis()
        {
        }

        public string Analysis { get; set; }
        public int BoardEvaluation { get; set; }
        public EvaluationType BoardEvaluationType { get; set; }
    }
}