using RedChess.ChessCommon.Enumerations;
using Redchess.Engine.Pieces.Abstract;

namespace Redchess.Engine.Pieces.White
{
    internal sealed class WhiteBishop : Bishop
    {
        internal WhiteBishop(Location loc)
            : base(loc, PieceType.WhiteBishop)
        {
        }
    }
}