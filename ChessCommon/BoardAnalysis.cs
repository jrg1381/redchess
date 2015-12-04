using System;
using RedChess.ChessCommon.Enumerations;
using RedChess.ChessCommon.Interfaces;

namespace RedChess.ChessCommon
{
    /// <summary>
    /// A board analysis represents the raw stockfish output.
    /// </summary>
    public class BoardAnalysis : IBoardAnalysis
    {
        /// <summary>
        /// Raw analysis string from the stockfish engine
        /// </summary>
        public string Analysis { get; set; }
        /// <summary>
        /// The evaluation in centipawns or number of steps until mate. Positive numbers indicate advantage for white.
        /// </summary>
        public int BoardEvaluation { get; set; }
        /// <summary>
        /// Enumeration indicating whether BoardEvaluation is a centipawn or mate-in-N score
        /// </summary>
        public EvaluationType BoardEvaluationType { get; set; }

        /// <summary>
        /// Copy a board analysis from another.
        /// </summary>
        /// <param name="bestMove"></param>
        public BoardAnalysis(IBoardAnalysis bestMove)
        {
            Analysis = bestMove.Analysis;
            BoardEvaluation = bestMove.BoardEvaluation;
            BoardEvaluationType = bestMove.BoardEvaluationType;
        }

        public BoardAnalysis()
        {
        }

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