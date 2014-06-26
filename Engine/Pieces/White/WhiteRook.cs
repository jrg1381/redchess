using RedChess.ChessCommon.Enumerations;
using Redchess.Engine.Pieces.Abstract;

namespace Redchess.Engine.Pieces.White
{
    internal sealed class WhiteRook : Rook
    {
        internal WhiteRook(Location loc)
            : base(loc, PieceType.WhiteRook)
        {
        }
    }
}