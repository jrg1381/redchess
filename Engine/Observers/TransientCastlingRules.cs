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
        private static readonly Location[] WhiteKingCastlingSquares = { Location.E1, Location.F1, Location.G1 };
        private static readonly Location[] WhiteKingCastlingSquaresEmpty = {Location.F1, Location.G1};
        private static readonly Location[] WhiteQueenCastlingSquaresEmpty = {Location.B1, Location.C1, Location.D1};
        private static readonly Location[] WhiteQueenCastlingSquares = {Location.C1, Location.D1, Location.E1};
        private static readonly Location[] BlackKingCastlingSquares = {Location.F8, Location.G8};
        private static readonly Location[] BlackKingCastlingSquareEmpty = {Location.E8, Location.F8, Location.G8};
        private static readonly Location[] BlackQueenCastlingSquaresEmpty = {Location.B8, Location.C8, Location.D8};
        private static readonly Location[] BlackQueenCastlingSquares = {Location.C8, Location.D8, Location.E8};

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
                            return SquaresEmpty(WhiteKingCastlingSquaresEmpty) &&
                                   SquaresNotAttacked(color, WhiteKingCastlingSquares);
                        case Side.QueenSide:
                            return SquaresEmpty(WhiteQueenCastlingSquaresEmpty) &&
                                   SquaresNotAttacked(color, WhiteQueenCastlingSquares);
                    }
                    break;
                case PieceColor.Black:
                {
                    switch (sideOfBoard)
                    {
                        case Side.KingSide:
                            return SquaresEmpty(BlackKingCastlingSquares) &&
                                   SquaresNotAttacked(color, BlackKingCastlingSquareEmpty);
                        case Side.QueenSide:
                            return SquaresEmpty(BlackQueenCastlingSquaresEmpty) &&
                                   SquaresNotAttacked(color, BlackQueenCastlingSquares);
                    }
                    break;
                }
            }

            throw new ArgumentException("Parameters to MayCastle made no sense");
        }
    }
}