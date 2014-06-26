using RedChess.ChessCommon.Enumerations;

namespace Redchess.Engine.Pieces.Abstract
{
    public static class ExtensionMethods
    {
        internal static PieceColor Color(this PieceType pieceType)
        {
            return (pieceType & PieceType.Black) > 0 ? PieceColor.Black : PieceColor.White;
        }
    }
}