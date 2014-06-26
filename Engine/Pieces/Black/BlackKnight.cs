using RedChess.ChessCommon.Enumerations;
using Redchess.Engine.Pieces.Abstract;

namespace Redchess.Engine.Pieces.Black
{
    internal sealed class BlackKnight : Knight
    {
        internal BlackKnight(Location loc)
            : base(loc, PieceType.BlackKnight)
        {
        }
    }
}