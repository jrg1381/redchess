using System;
using RedChess.ChessCommon.Enumerations;
using Redchess.Engine.Interfaces;

namespace Redchess.Engine
{
    sealed class PermanentCastlingRules : AbstractBoardObserver<CastlingOptions>
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
            if (!DataIsCurrent)
            {
                UpdateValue();
                DataIsCurrent = true;
            }

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
}