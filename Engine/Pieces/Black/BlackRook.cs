using RedChess.ChessCommon.Enumerations;
using Redchess.Engine.Pieces.Abstract;

namespace Redchess.Engine.Pieces.Black
{
    internal sealed class BlackRook : Rook
    {
        internal BlackRook(Location loc)
            : base(loc, PieceType.BlackRook)
        {
        }
    }
}