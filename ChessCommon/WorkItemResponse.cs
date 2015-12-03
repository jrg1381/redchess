using System;
using System.Collections.Generic;
using RedChess.ChessCommon.Enumerations;
using RedChess.ChessCommon.Interfaces;

namespace RedChess.ChessCommon
{
    public class ProcessedAnalysis : IProcessedAnalysis
    {
        public int BoardEvaluation { get; set; }
        public EvaluationType BoardEvaluationType { get; set; }
        public IList<IHistoryEntry> Analysis { get; private set; }

        public ProcessedAnalysis(IBoardAnalysis boardAnalysis)
        {
            BoardEvaluationType = boardAnalysis.BoardEvaluationType;
            BoardEvaluation = boardAnalysis.BoardEvaluation;
            Analysis = new List<IHistoryEntry>();
        }
    }

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

        public override string ToString()
        {
            return String.Format("{0}:{1}:{2}", BoardEvaluation, BoardEvaluationType, Analysis);
        }

        public override bool Equals(object obj)
        {
            var analysis = obj as BoardAnalysis;

            if (analysis != null)
            {
                return Equals(analysis);
            }

            return false;
        }

        protected bool Equals(BoardAnalysis other)
        {
            return string.Equals(Analysis, other.Analysis) && BoardEvaluation == other.BoardEvaluation && BoardEvaluationType == other.BoardEvaluationType;
        }

        public override int GetHashCode()
        {
            unchecked
            {
                var hashCode = (Analysis != null ? Analysis.GetHashCode() : 0);
                hashCode = (hashCode*397) ^ BoardEvaluation;
                hashCode = (hashCode*397) ^ (int) BoardEvaluationType;
                return hashCode;
            }
        }
    }
}