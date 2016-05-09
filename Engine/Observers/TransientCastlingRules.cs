using System;
using System.Collections.Generic;
using System.Linq;
using Redchess.Engine.Interfaces;
using Redchess.Engine.Structures;
using RedChess.ChessCommon.Enumerations;

namespace Redchess.Engine.Observers
{
    sealed class TransientCastlingRules : AbstractBoardObserver<CastlingOptions>
    {
        private static readonly Location[] s_WhiteKingCastlingSquares = { Location.E1, Location.F1, Location.G1 };
        private static readonly Location[] s_WhiteKingCastlingSquaresEmpty = {Location.F1, Location.G1};
        private static readonly Location[] s_WhiteQueenCastlingSquaresEmpty = {Location.B1, Location.C1, Location.D1};
        private static readonly Location[] s_WhiteQueenCastlingSquares = {Location.C1, Location.D1, Location.E1};
        private static readonly Location[] s_BlackKingCastlingSquares = {Location.F8, Location.G8};
        private static readonly Location[] s_BlackKingCastlingSquareEmpty = {Location.E8, Location.F8, Location.G8};
        private static readonly Location[] s_BlackQueenCastlingSquaresEmpty = {Location.B8, Location.C8, Location.D8};
        private static readonly Location[] s_BlackQueenCastlingSquares = {Location.C8, Location.D8, Location.E8};

        internal TransientCastlingRules(IBoardExtended board) : base(board)
        {
        }

        protected override void UpdateValue()
        {
            var temp = CastlingOptions.None;

            if (MayCastle(PieceColor.Black, Side.KingSide))
                temp |= CastlingOptions.BlackKingSide;
            if (MayCastle(PieceColor.White, Side.KingSide))
                temp |= CastlingOptions.WhiteKingSide;
            if (MayCastle(PieceColor.Black, Side.QueenSide))
                temp |= CastlingOptions.BlackQueenSide;
            if (MayCastle(PieceColor.White, Side.QueenSide))
                temp |= CastlingOptions.WhiteQueenSide;

            Value = temp;
        }

        /// <summary>
        ///     Returns true if all the supplied squares are empty
        /// </summary>
        /// <param name="squares">Set of squares to check</param>
        /// <returns></returns>
        private bool SquaresEmpty(IEnumerable<Location> squares)
        {
            return squares.All(s => Board.GetContents(s) == null);
        }

        /// <summary>
        ///     Returns true if none of the supplied squares are attacked by the opponent's pieces
        /// </summary>
        /// <param name="color">The friendly side</param>
        /// <param name="squares">Set of squares to check</param>
        /// <returns></returns>
        private bool SquaresNotAttacked(PieceColor color, IEnumerable<Location> squares)
        {
            return squares.All(s => !new CheckTester(color,s,Board).Check());
        }

        /// <summary>
        ///     Returns true if the
        ///     <param name="color">color</param>
        ///     player can castle on the specified side of the board
        /// </summary>
        /// <param name="color"></param>
        /// <param name="sideOfBoard"></param>
        /// <returns></returns>
        private bool MayCastle(PieceColor color, Side sideOfBoard)
        {
            switch (color)
            {
                case PieceColor.White:
                    switch (sideOfBoard)
                    {
                        case Side.KingSide:
                            return SquaresEmpty(s_WhiteKingCastlingSquaresEmpty) &&
                                   SquaresNotAttacked(color, s_WhiteKingCastlingSquares);
                        case Side.QueenSide:
                            return SquaresEmpty(s_WhiteQueenCastlingSquaresEmpty) &&
                                   SquaresNotAttacked(color, s_WhiteQueenCastlingSquares);
                    }
                    break;
                case PieceColor.Black:
                {
                    switch (sideOfBoard)
                    {
                        case Side.KingSide:
                            return SquaresEmpty(s_BlackKingCastlingSquares) &&
                                   SquaresNotAttacked(color, s_BlackKingCastlingSquareEmpty);
                        case Side.QueenSide:
                            return SquaresEmpty(s_BlackQueenCastlingSquaresEmpty) &&
                                   SquaresNotAttacked(color, s_BlackQueenCastlingSquares);
                    }
                    break;
                }
            }

            throw new ArgumentException("Parameters to MayCastle made no sense");
        }
    }
}