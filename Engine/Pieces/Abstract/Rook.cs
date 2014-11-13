using System;
using System.Collections.Generic;
using RedChess.ChessCommon.Enumerations;
using Redchess.Engine.Interfaces;

namespace Redchess.Engine.Pieces.Abstract
{
    internal abstract class Rook : Piece
    {
        private static readonly Tuple<int, int>[] s_moveSet = {new Tuple<int, int>(0, 1), 
                                                               new Tuple<int, int>(0, -1), 
                                                               new Tuple<int, int>(1, 0),
                                                               new Tuple<int, int>(-1, 0)};

        protected Rook(Location loc, PieceType pieceType)
            : base(loc, pieceType)
        {
        }

        public override IEnumerable<Location> ReachableSquares(IBoardExtended game)
        {
            return ReachableSquares(s_moveSet, game);
        }
    }
}