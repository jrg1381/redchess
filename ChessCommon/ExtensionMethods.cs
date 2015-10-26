using RedChess.ChessCommon.Enumerations;

namespace RedChess.ChessCommon
{
    public static class ExtensionMethods
    {
        private static readonly PieceType Colors = PieceType.Black | PieceType.White;
        private static readonly PieceType NotColors = ~Colors;

        public static bool IsOfType(this PieceType pieceType, PieceType targetPieceType)
        {
            // Test is comparing with an 'abstract' piece, King, Queen, Pawn, etc
            if ((targetPieceType & Colors) == PieceType.None)
                return ((pieceType & targetPieceType) > Colors);

            // Test is for a specific color of piece
            return pieceType == targetPieceType;
        }
    }
}
