using System;
using System.Collections.Generic;
using System.Linq;
using RedChess.ChessCommon.Enumerations;
using Redchess.Engine.Interfaces;

namespace Redchess.Engine
{
    [Flags]
    public enum CastlingOptions
    {
        None = 0,
        BlackQueenSide = 1,
        BlackKingSide = 2,
        WhiteKingSide = 4,
        WhiteQueenSide = 8,
        All = BlackQueenSide | BlackKingSide | WhiteKingSide | WhiteQueenSide
    }

    sealed class PermanentCastlingRules : AbstractBoardObserver2<CastlingOptions>
    {
        internal PermanentCastlingRules(IBoardExtended board)
            : base(board)
        {
            Value = CastlingOptions.All;
        }

        public PermanentCastlingRules(IBoardExtended board, Board replacementBoard) : base(board)
        {
            Value = replacementBoard.PermanentCastlingOptions;
        }

        protected override void UpdateValue()
        {
            var castlingFlags = m_data;

            if (Board.PreviousState == null)
                return;

            var piece = Board.PreviousState.MovedPiece;
            var originalLocation = piece.Position.Location;
            var newLocation = Board.PreviousState.Target;

            // A white piece has moved, potentially taking the black rooks
            if (piece.Color == PieceColor.White)
            {
                if (newLocation == Location.A8)
                {
                    castlingFlags ^= CastlingOptions.BlackQueenSide;
                }

                if (newLocation == Location.H8)
                {
                    castlingFlags ^= CastlingOptions.BlackKingSide;
                }
            }

            // A black piece has moved, potentially taking the white rooks
            if (piece.Color == PieceColor.Black)
            {
                if (newLocation == Location.A1)
                {
                    castlingFlags ^= CastlingOptions.WhiteQueenSide;
                }

                if (newLocation == Location.H1)
                {
                    castlingFlags ^= CastlingOptions.WhiteKingSide;
                }
            }

            // A rook or a king has moved, update castling options accordingly
            switch (piece.Type)
            {
                case PieceType.WhiteKing:
                {
                    if (castlingFlags.HasFlag(CastlingOptions.WhiteQueenSide))
                        castlingFlags ^= CastlingOptions.WhiteQueenSide;
                    if (castlingFlags.HasFlag(CastlingOptions.WhiteKingSide))
                        castlingFlags ^= CastlingOptions.WhiteKingSide;
                    break;
                }
                case PieceType.BlackKing:
                {
                    if (castlingFlags.HasFlag(CastlingOptions.BlackQueenSide))
                        castlingFlags ^= CastlingOptions.BlackQueenSide;
                    if (castlingFlags.HasFlag(CastlingOptions.BlackKingSide))
                        castlingFlags ^= CastlingOptions.BlackKingSide;
                    break;
                }
                case PieceType.BlackRook:
                    if (originalLocation == Location.H8 && castlingFlags.HasFlag(CastlingOptions.BlackKingSide))
                        castlingFlags ^= CastlingOptions.BlackKingSide;
                    if (originalLocation == Location.A8 && castlingFlags.HasFlag(CastlingOptions.BlackQueenSide))
                        castlingFlags ^= CastlingOptions.BlackQueenSide;
                    break;
                case PieceType.WhiteRook:
                    if (originalLocation == Location.H1 && castlingFlags.HasFlag(CastlingOptions.WhiteKingSide))
                        castlingFlags ^= CastlingOptions.WhiteKingSide;
                    if (originalLocation == Location.A1 && castlingFlags.HasFlag(CastlingOptions.WhiteQueenSide))
                        castlingFlags ^= CastlingOptions.WhiteQueenSide;
                    break;
            }

            m_data = castlingFlags;
        }

        /// <summary>
        ///     Return a fen substring ("KkQq") representing the current castling state
        /// </summary>
        /// <returns></returns>
        internal string FenCastleString()
        {
            string whiteKingside = Value.HasFlag(CastlingOptions.WhiteKingSide) ? "K" : "";
            string whiteQueenside = Value.HasFlag(CastlingOptions.WhiteQueenSide) ? "Q" : "";
            string blackKingside = Value.HasFlag(CastlingOptions.BlackKingSide) ? "k" : "";
            string blackQueenside = Value.HasFlag(CastlingOptions.BlackQueenSide) ? "q" : "";

            string answer = String.Format("{0}{1}{2}{3}", whiteKingside, whiteQueenside, blackKingside, blackQueenside);

            return String.IsNullOrEmpty(answer) ? "-" : answer;
        }

        /// <summary>
        ///     Update the castling settings from a fen substring (something like "KkQq") where K indicates castling White
        ///     king-side, etc.
        /// </summary>
        /// <param name="fenSubstring"></param>
        internal void UpdateFromFen(string fenSubstring)
        {
            m_data = CastlingOptions.None;

            if (fenSubstring.Contains("K"))
                m_data |= CastlingOptions.WhiteKingSide;
            if (fenSubstring.Contains("k"))
                m_data |= CastlingOptions.BlackKingSide;
            if (fenSubstring.Contains("Q"))
                m_data |= CastlingOptions.WhiteQueenSide;
            if (fenSubstring.Contains("q"))
                m_data |= CastlingOptions.BlackQueenSide;

            DataIsCurrent = true;
        }
    }

    sealed class CastlingRules : AbstractBoardObserver2<CastlingOptions>
    {
        private CastlingOptions m_permanentCastlingFlags = CastlingOptions.All;

        internal CastlingRules(IBoardExtended board) : base(board)
        {
        }

        protected override void UpdateValue()
        {
            m_permanentCastlingFlags = Board.PermanentCastlingOptions;
            // Nothing to do
            if (m_permanentCastlingFlags == CastlingOptions.None)
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
                            return m_permanentCastlingFlags.HasFlag(CastlingOptions.WhiteKingSide) && SquaresEmpty(new[] {Location.F1, Location.G1}) &&
                                   SquaresNotAttacked(color, new[] { Location.E1, Location.F1, Location.G1 });
                        case Side.QueenSide:
                            return m_permanentCastlingFlags.HasFlag(CastlingOptions.WhiteQueenSide) &&
                                   SquaresEmpty(new[] { Location.B1, Location.C1, Location.D1 }) &&
                                   SquaresNotAttacked(color, new[] { Location.C1, Location.D1, Location.E1 });
                    }
                    break;
                case PieceColor.Black:
                {
                    switch (sideOfBoard)
                    {
                        case Side.KingSide:
                            return m_permanentCastlingFlags.HasFlag(CastlingOptions.BlackKingSide) && SquaresEmpty(new[] { Location.F8, Location.G8 }) &&
                                   SquaresNotAttacked(color, new[] { Location.E8, Location.F8, Location.G8 });
                        case Side.QueenSide:
                            return m_permanentCastlingFlags.HasFlag(CastlingOptions.BlackQueenSide) &&
                                   SquaresEmpty(new[] { Location.B8, Location.C8, Location.D8 }) &&
                                   SquaresNotAttacked(color, new[] { Location.C8, Location.D8, Location.E8 });
                    }
                }
                    break;
            }

            throw new ArgumentException("Parameters to MayCastle made no sense");
        }
    }
}