using RedChess.ChessCommon.Enumerations;
using Redchess.Engine.Pieces.Abstract;

namespace Redchess.Engine.Pieces.Black
{
    internal sealed class BlackPawn : Pawn
    {
        internal BlackPawn(Location loc)
            : base(loc, PieceType.BlackPawn)
        {
            DirectionOfTravel = -1;
            StartRow = 6;
        }
    }
}