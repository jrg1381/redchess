using System;
using System.Collections.Generic;
using System.Linq;
using RedChess.ChessCommon.Enumerations;
using Redchess.Engine.Interfaces;

namespace Redchess.Engine
{
    sealed class TransientCastlingRules : AbstractBoardObserver<CastlingOptions>
    {
        internal TransientCastlingRules(IBoardExtended board) : base(board)
        {
        }

        protected override void UpdateValue()
        {
            // Nothing to do
            if (Board.PermanentCastlingOptions == CastlingOptions.None)
                return;

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
            return squares.All(s => !Board.KingInCheck(color, s));
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
                            return Board.PermanentCastlingOptions.HasFlag(CastlingOptions.WhiteKingSide) &&
                                   SquaresEmpty(new[] {Location.F1, Location.G1}) &&
                                   SquaresNotAttacked(color, new[] {Location.E1, Location.F1, Location.G1});
                        case Side.QueenSide:
                            return Board.PermanentCastlingOptions.HasFlag(CastlingOptions.WhiteQueenSide) &&
                                   SquaresEmpty(new[] {Location.B1, Location.C1, Location.D1}) &&
                                   SquaresNotAttacked(color, new[] {Location.C1, Location.D1, Location.E1});
                    }
                    break;
                case PieceColor.Black:
                {
                    switch (sideOfBoard)
                    {
                        case Side.KingSide:
                            return Board.PermanentCastlingOptions.HasFlag(CastlingOptions.BlackKingSide) &&
                                   SquaresEmpty(new[] {Location.F8, Location.G8}) &&
                                   SquaresNotAttacked(color, new[] {Location.E8, Location.F8, Location.G8});
                        case Side.QueenSide:
                            return Board.PermanentCastlingOptions.HasFlag(CastlingOptions.BlackQueenSide) &&
                                   SquaresEmpty(new[] {Location.B8, Location.C8, Location.D8}) &&
                                   SquaresNotAttacked(color, new[] {Location.C8, Location.D8, Location.E8});
                    }
                    break;
                }
            }

            throw new ArgumentException("Parameters to MayCastle made no sense");
        }
    }
}