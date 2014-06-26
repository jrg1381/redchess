using System.Collections.Generic;
using System.Linq;
using RedChess.ChessCommon.Enumerations;
using Redchess.Engine.Interfaces;
using Redchess.Engine.Pieces.Abstract;

namespace Redchess.Engine.Pieces.Black
{
    internal sealed class BlackKing : King
    {
        internal BlackKing(Location loc)
            : base(loc, PieceType.BlackKing)
        {
        }

        public override IEnumerable<Location> ReachableSquares(IBoardExtended game)
        {
            var extraSquares = new List<Location>();

            if (game.MayCastle(this, Side.KingSide))
            {
                extraSquares.Add(Location.G8);
            }
            if (game.MayCastle(this, Side.QueenSide))
            {
                extraSquares.Add(Location.C8);
            }

            return base.ReachableSquares(game).Concat(extraSquares);
        }
    }
}