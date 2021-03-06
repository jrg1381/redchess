﻿using System.Collections.Generic;
using RedChess.ChessCommon.Enumerations;
using RedChess.ChessCommon.Interfaces;

namespace RedChess.WebEngine.Repositories
{
    class AnalysisBinding : IAnalysisBinding
    {
        internal static AnalysisBinding Empty = new AnalysisBinding();

        public IList<IHistoryEntry> AnalysisLines { get; set; }
        public int Evaluation { get; set; }
        public EvaluationType BoardEvaluationType { get; set; }
        public int MoveNumber { get; set; }

        public bool IsEmpty()
        {
            return AnalysisLines.Count != 0;
        }
    }
}