using System;
using System.Diagnostics;
using RedChess.ChessCommon.Enumerations;

namespace RedChess.ChessCommon
{
    public static class ExtensionMethods
    {
        private const PieceType c_colors = PieceType.Black | PieceType.White;

        public static bool IsOfType(this PieceType pieceType, PieceType targetPieceType)
        {
            // Test is comparing with an 'abstract' piece, King, Queen, Pawn, etc
            if ((targetPieceType & c_colors) == PieceType.None)
                return ((pieceType & targetPieceType) > c_colors);

            // Test is for a specific color of piece
            return pieceType == targetPieceType;
        }

        /// <summary>
        /// Returns true if gameStatus refers to a game over state, false otherwise
        /// </summary>
        /// <param name="gameStatus"></param>
        /// <returns></returns>
        public static bool GameOver(this GameStatus gameStatus)
        {
            return gameStatus > GameStatus.Check;
        }

        /// <summary>
        /// Turns the enum into a string suitable for UI display
        /// </summary>
        /// <param name="gameStatus"></param>
        /// <returns></returns>
        public static string FriendlyName(this GameStatus gameStatus)
        {
            switch (gameStatus)
            {
                case GameStatus.None:
                    return String.Empty;
                case GameStatus.Check:
                    return "Check";
                case GameStatus.CheckmateWhiteWins:
                case GameStatus.CheckmateBlackWins:
                    return "Checkmate";
                case GameStatus.Stalemate:
                    return "Stalemate";
                case GameStatus.DrawInsufficientMaterial:
                    return "Draw - insufficient material";
                default:
                    throw new ArgumentOutOfRangeException(nameof(gameStatus));
            }
        }
    }
}
