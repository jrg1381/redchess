using System.Collections.Generic;
using System.Linq;
using RedChess.ChessCommon.Enumerations;
using Redchess.Engine.Interfaces;
using Redchess.Engine.Pieces.Abstract;

namespace Redchess.Engine.Pieces.White
{
    internal sealed class WhiteKing : King
    {
        internal WhiteKing(Location loc)
            : base(loc, PieceType.WhiteKing)
        {
        }

        public override IEnumerable<Location> ReachableSquares(IBoardExtended game)
        {
            var extraSquares = new List<Location>();

            if (game.MayCastle(this, Side.KingSide))
            {
                extraSquares.Add(Location.G1);
            }
            if (game.MayCastle(this, Side.QueenSide))
            {
                extraSquares.Add(Location.C1);
            }

            return base.ReachableSquares(game).Concat(extraSquares);
        }
    }
}