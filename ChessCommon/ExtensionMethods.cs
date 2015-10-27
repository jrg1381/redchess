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
    }
}
