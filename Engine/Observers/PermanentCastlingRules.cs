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
            var castlingFlags = Data;

            if (Board.PreviousState == null)
                return;

            var piece = Board.PreviousState.MovedPiece;
            var originalLocation = piece.Position.Location;
            var newLocation = Board.PreviousState.Target;

            switch (piece.Color)
            {
                case PieceColor.Black: // A black piece has moved, potentially taking the white rooks
                    switch (newLocation)
                    {
                        case Location.A1:
                            castlingFlags &= ~CastlingOptions.WhiteQueenSide;
                            break;
                        case Location.H1:
                            castlingFlags &= ~CastlingOptions.WhiteKingSide;
                            break;
                    }
                    break;
                case PieceColor.White: // A white piece has moved, potentially taking the black rooks
                    switch (newLocation)
                    {
                        case Location.A8:
                            castlingFlags &= ~CastlingOptions.BlackQueenSide;
                            break;
                        case Location.H8:
                            castlingFlags &= ~CastlingOptions.BlackKingSide;
                            break;
                    }
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
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

            Data = castlingFlags;
        }

        /// <summary>
        ///     Update the castling settings from a fen substring (something like "KkQq") where K indicates castling White
        ///     king-side, etc.
        /// </summary>
        /// <param name="fenSubstring"></param>
        internal void UpdateFromFen(string fenSubstring)
        {
            Data = CastlingOptions.None;

            foreach (var c in fenSubstring)
            {
                switch (c)
                {
                    case 'K':
                        Data |= CastlingOptions.WhiteKingSide;
                        break;
                    case 'k':
                        Data |= CastlingOptions.BlackKingSide;
                        break;
                    case 'Q':
                        Data |= CastlingOptions.WhiteQueenSide;
                        break;
                    case 'q':
                        Data |= CastlingOptions.BlackQueenSide;
                        break;
                }
            }

            DataIsCurrent = true;
        }
    }
}