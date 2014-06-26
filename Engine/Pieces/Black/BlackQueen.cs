using RedChess.ChessCommon.Enumerations;
using Redchess.Engine.Pieces.Abstract;

namespace Redchess.Engine.Pieces.Black
{
    internal sealed class BlackQueen : Queen
    {
        internal BlackQueen(Location loc)
            : base(loc, PieceType.BlackQueen)
        {
        }
    }
}