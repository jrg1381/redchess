using System.Collections.Generic;
using RedChess.ChessCommon.Enumerations;

namespace RedChess.ChessCommon.Interfaces
{
    public interface IBoardAnalysis
    {
        string Analysis { get; set; }
        int BoardEvaluation { get; set; }
        EvaluationType BoardEvaluationType { get; set; }
    }

    public interface IProcessedAnalysis
    {
        int BoardEvaluation { get; set; }
        EvaluationType BoardEvaluationType { get; set; }
        IList<IHistoryEntry> Analysis { get; }
    }
}