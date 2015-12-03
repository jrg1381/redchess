﻿using System;
using System.Collections.Generic;
using RedChess.ChessCommon.Enumerations;
using RedChess.ChessCommon.Interfaces;
using RedChess.WebEngine.Models;

namespace RedChess.WebEngine.Repositories
{
    class AnalysisBinding : IAnalysisBinding
    {
        internal static AnalysisBinding Empty = new AnalysisBinding();

        public IList<IHistoryEntry> Analysis { get; set; }
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