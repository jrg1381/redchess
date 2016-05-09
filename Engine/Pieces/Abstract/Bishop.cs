using System.Collections.Generic;
using RedChess.ChessCommon.Enumerations;
using Redchess.Engine.Interfaces;

namespace Redchess.Engine.Pieces.Abstract
{
    internal abstract class Bishop : Piece
    {
        private static readonly Vector2[] s_MoveSet =
        {
            new Vector2(1, -1),
            new Vector2(1, 1),
            new Vector2(-1, 1),
            new Vector2(-1, -1)
        };

        protected Bishop(Location loc, PieceType pieceType)
            : base(loc, pieceType)
        {
        }

        public override IEnumerable<Location> ReachableSquares(IBoardExtended game)
        {
            return ReachableSquares(s_MoveSet, game);
        }
    }
}