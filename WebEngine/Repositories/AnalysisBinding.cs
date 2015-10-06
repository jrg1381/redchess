using System;
using RedChess.ChessCommon.Enumerations;

namespace RedChess.WebEngine.Repositories
{
    class AnalysisBinding : IAnalysisBinding
    {
        internal static AnalysisBinding Empty = new AnalysisBinding();

        public string AnalysisText { get; set; }
        public int Evaluation { get; set; }
        public EvaluationType BoardEvaluationType { get; set; }
        public int MoveNumber { get; set; }

        public bool IsEmpty()
        {
            return String.IsNullOrEmpty(AnalysisText);
        }
    }
}