using System.Collections.Generic;
using RedChess.ChessCommon.Enumerations;
using Redchess.Engine.Interfaces;

namespace Redchess.Engine.Pieces.Abstract
{
    internal abstract class King : Piece
    {
        private static readonly Vector2[] MoveSet =
        {
            new Vector2(0, 1),
            new Vector2(0, -1),
            new Vector2(1, 1),
            new Vector2(1, -1),
            new Vector2(1, 0),
            new Vector2(-1, 0),
            new Vector2(-1, 1),
            new Vector2(-1, -1)
        };

        protected King(Location loc, PieceType pieceType)
            : base(loc, pieceType)
        {
        }

        public override IEnumerable<Location> AttackedSquares(IBoardExtended game)
        {
            // Important not to use the WhiteKing / BlackKing implementation of ReachableSquares, because that includes squares reachable by castling
            return ReachableSquares(MoveSet, game);
        }

        public override IEnumerable<Location> ReachableSquares(IBoardExtended game)
        {
            return ReachableSquares(MoveSet, game);
        }
    }
}