using RedChess.ChessCommon.Enumerations;
using Redchess.Engine.Pieces.Abstract;

namespace Redchess.Engine.Pieces.White
{
    internal sealed class WhitePawn : Pawn
    {
        internal WhitePawn(Location loc)
            : base(loc, PieceType.WhitePawn)
        {
            DirectionOfTravel = +1;
            StartRow = 1;
        }
    }
}