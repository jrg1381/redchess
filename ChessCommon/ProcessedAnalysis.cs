using System;
using System.Collections.Generic;
using System.Linq;
using RedChess.ChessCommon.Enumerations;
using RedChess.ChessCommon.Interfaces;

namespace RedChess.ChessCommon
{
    public class ProcessedAnalysis : IProcessedAnalysis
    {
        public int BoardEvaluation { get; private set; }
        public EvaluationType BoardEvaluationType { get; private set; }
        public IList<IHistoryEntry> Analysis { get; private set; }

        public ProcessedAnalysis(IBoardAnalysis boardAnalysis)
        {
            BoardEvaluationType = boardAnalysis.BoardEvaluationType;
            BoardEvaluation = boardAnalysis.BoardEvaluation;
            Analysis = new List<IHistoryEntry>();
        }

        public override string ToString()
        {
            return String.Format("{0}:{1}:{2}:{3}:{4}",
                BoardEvaluation,
                BoardEvaluationType,
                String.Join(" ", Analysis.Select(x => x.Move)),
                String.Join(",", Analysis.Select(x => x.Fen)),
                String.Join(" ", Analysis.Select(x => x.MoveNumber))
                );
        }

        public override bool Equals(object obj)
        {
            var analysis = obj as ProcessedAnalysis;

            if (analysis != null)
            {
                return Equals(analysis);
            }

            return false;
        }

        public override int GetHashCode()
        {
            unchecked
            {
                var hashCode = BoardEvaluation;
                hashCode = (hashCode*397) ^ (int) BoardEvaluationType;
                hashCode = (hashCode*397) ^ (Analysis != null ? Analysis.GetHashCode() : 0);
                return hashCode;
            }
        }

        protected bool Equals(ProcessedAnalysis other)
        {
            return Analysis.SequenceEqual(other.Analysis, new HistoryEntryEqualityComparer()) && 
                   BoardEvaluation == other.BoardEvaluation && 
                   BoardEvaluationType == other.BoardEvaluationType;
        }
    }
}