using RedChess.ChessCommon.Enumerations;
using Redchess.Engine.Pieces.Abstract;

namespace Redchess.Engine.Pieces.Black
{
    internal sealed class BlackBishop : Bishop
    {
        internal BlackBishop(Location loc)
            : base(loc, PieceType.BlackBishop)
        {
        }
    }
}