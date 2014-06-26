using RedChess.ChessCommon.Enumerations;
using Redchess.Engine.Pieces.Abstract;

namespace Redchess.Engine.Pieces.White
{
    internal sealed class WhiteQueen : Queen
    {
        internal WhiteQueen(Location loc)
            : base(loc, PieceType.WhiteQueen)
        {
        }
    }
}