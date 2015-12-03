using System.Collections.Generic;
using RedChess.ChessCommon.Enumerations;

namespace RedChess.ChessCommon.Interfaces
{
    public interface IProcessedAnalysis
    {
        int BoardEvaluation { get; }
        EvaluationType BoardEvaluationType { get; }
        IList<IHistoryEntry> Analysis { get; }
    }
}