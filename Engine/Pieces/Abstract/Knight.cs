using System.Collections.Generic;
using RedChess.ChessCommon.Enumerations;
using Redchess.Engine.Interfaces;

namespace Redchess.Engine.Pieces.Abstract
{
    internal abstract class Knight : Piece
    {
        private static readonly int[] s_dxSet = { 1, 2, -1, -2, 1, -1, -2, -1, 2 };
        private static readonly int[] s_dySet = { 2, 1, 2, 1, -2, -2, -1, 2, -1 };

        protected Knight(Location loc, PieceType pieceType)
            : base(loc, pieceType)
        {
        }

        public override IEnumerable<Location> ReachableSquares(IBoardExtended game)
        {
            return ReachableSquares(s_dxSet, s_dySet, game);
        }
    }
}