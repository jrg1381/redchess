using System;
using Redchess.Engine.Interfaces;
using Redchess.Engine.Structures;
using RedChess.ChessCommon.Enumerations;

namespace Redchess.Engine.Observers
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
                        castlingFlags &= ~CastlingOptions.BlackQueenSide;
                }

                if (newLocation == Location.H8)
                {
                    castlingFlags &= ~CastlingOptions.BlackKingSide;
                }
            }

            // A black piece has moved, potentially taking the white rooks
            if (piece.Color == PieceColor.Black)
            {
                if (newLocation == Location.A1)
                {
                    castlingFlags &= ~CastlingOptions.WhiteQueenSide;
                }

                if (newLocation == Location.H1)
                {
                    castlingFlags &= ~CastlingOptions.WhiteKingSide;
                }
            }

            // A rook or a king has moved, update castling options accordingly
            switch (piece.Type)
            {
                case PieceType.WhiteKing:
                {
                    castlingFlags &= ~CastlingOptions.WhiteQueenSide;
                    castlingFlags &= ~CastlingOptions.WhiteKingSide;
                    break;
                }
                case PieceType.BlackKing:
                {
                    castlingFlags &= ~CastlingOptions.BlackQueenSide;
                    castlingFlags &= ~CastlingOptions.BlackKingSide;
                    break;
                }
                case PieceType.BlackRook:
                    if (originalLocation == Location.H8)
                        castlingFlags &= ~CastlingOptions.BlackKingSide;
                    if (originalLocation == Location.A8)
                        castlingFlags &= ~CastlingOptions.BlackQueenSide;
                    break;
                case PieceType.WhiteRook:
                    if (originalLocation == Location.H1)
                        castlingFlags &= ~CastlingOptions.WhiteKingSide;
                    if (originalLocation == Location.A1)
                        castlingFlags &= ~CastlingOptions.WhiteQueenSide;
                    break;
            }

            m_data = castlingFlags;
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