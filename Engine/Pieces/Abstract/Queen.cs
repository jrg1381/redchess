using System.Collections.Generic;
using RedChess.ChessCommon.Enumerations;
using Redchess.Engine.Interfaces;

namespace Redchess.Engine.Pieces.Abstract
{
    internal abstract class Queen : Piece
    {
        private static readonly int[] s_dxSet = { -1, 1, -1, 1, 0, 0, 1, -1 };
        private static readonly int[] s_dySet = { -1, 1, 1, -1, 1, -1, 0, 0 };
        
        protected Queen(Location loc, PieceType pieceType)
            : base(loc, pieceType)
        {
        }

        public override IEnumerable<Location> ReachableSquares(IBoardExtended game)
        {
            return ReachableSquares(s_dxSet, s_dySet, game);
        }
    }
}