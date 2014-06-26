using RedChess.ChessCommon.Enumerations;

namespace RedChess.ChessCommon
{
    public static class ExtensionMethods
    {
        public static bool IsOfType(this PieceType pieceType, PieceType targetPieceType)
        {
            return (pieceType & targetPieceType) > (PieceType.RealPiece | PieceType.Black);
        }
    }
}
