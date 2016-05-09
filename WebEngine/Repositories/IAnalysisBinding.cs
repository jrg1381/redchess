using System.Collections.Generic;
using RedChess.ChessCommon.Enumerations;
using RedChess.ChessCommon.Interfaces;

namespace RedChess.WebEngine.Repositories
{
    public interface IAnalysisBinding
    {
        IList<IHistoryEntry> AnalysisLines { get; set; }
        int Evaluation { get; set; }
        EvaluationType BoardEvaluationType { get; set; }
        int MoveNumber { get; set; }
        bool IsEmpty();
    }
}