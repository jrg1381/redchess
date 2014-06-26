using System.Collections.Generic;
using RedChess.ChessCommon.Enumerations;
using Redchess.Engine.Interfaces;

namespace Redchess.Engine.Pieces.Abstract
{
    internal abstract class King : Piece
    {
        private readonly static int[] s_dxSet = {0, 0, 1, 1, 1, -1, -1, -1};
        private readonly static int[] s_dySet = {1, -1, 1, -1, 0, 0, 1, -1};

        protected King(Location loc, PieceType pieceType)
            : base(loc, pieceType)
        {
        }

        public override IEnumerable<Location> AttackedSquares(IBoardExtended game)
        {
            // Important not to use the WhiteKing / BlackKing implementation of ReachableSquares, because that includes squares reachable by castling
            return ReachableSquares(s_dxSet, s_dySet, game);
        }

        public override IEnumerable<Location> ReachableSquares(IBoardExtended game)
        {
            return ReachableSquares(s_dxSet, s_dySet, game);
        }
    }
}