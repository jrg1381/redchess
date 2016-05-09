using RedChess.ChessCommon.Enumerations;
using Redchess.Engine.Interfaces;
using Redchess.Engine.Pieces.Black;
using Redchess.Engine.Pieces.White;

namespace Redchess.Engine.Pieces
{
    internal static class PieceFactory
    {
        internal static IPiece CreatePiece(PieceType typeOfPiece, Location loc)
        {
            switch (typeOfPiece)
            {
                case PieceType.WhiteKing:
                    return new WhiteKing(loc);
                case PieceType.BlackKing:
                    return new BlackKing(loc);
                case PieceType.WhiteQueen:
                    return new WhiteQueen(loc);
                case PieceType.BlackQueen:
                    return new BlackQueen(loc);
                case PieceType.WhiteRook:
                    return new WhiteRook(loc);
                case PieceType.BlackRook:
                    return new BlackRook(loc);
                case PieceType.WhiteBishop:
                    return new WhiteBishop(loc);
                case PieceType.BlackBishop:
                    return new BlackBishop(loc);
                case PieceType.WhiteKnight:
                    return new WhiteKnight(loc);
                case PieceType.BlackKnight:
                    return new BlackKnight(loc);
                case PieceType.WhitePawn:
                    return new WhitePawn(loc);
                case PieceType.BlackPawn:
                    return new BlackPawn(loc);
                default:
                    return null;
            }
        }
    }
}