using RedChess.ChessCommon.Enumerations;
using Redchess.Engine.Pieces.Abstract;

namespace Redchess.Engine.Pieces.White
{
    internal sealed class WhiteKnight : Knight
    {
        internal WhiteKnight(Location loc)
            : base(loc, PieceType.WhiteKnight)
        {
        }
    }
}