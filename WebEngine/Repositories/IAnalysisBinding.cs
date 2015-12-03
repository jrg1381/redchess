using System.Collections;
using System.Collections.Generic;
using RedChess.ChessCommon.Enumerations;
using RedChess.ChessCommon.Interfaces;
using RedChess.WebEngine.Models;

namespace RedChess.WebEngine.Repositories
{
    public interface IAnalysisBinding
    {
        IList<IHistoryEntry> Analysis { get; set; }
        string AnalysisText { get; set; }
        int Evaluation { get; set; }
        EvaluationType BoardEvaluationType { get; set; }
        int MoveNumber { get; set; }
        bool IsEmpty();
    }
}