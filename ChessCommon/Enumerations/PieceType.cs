using System;

namespace RedChess.ChessCommon.Enumerations
{
    /// <summary>
    /// The idea here is that the lowest bit represents the colour (white = 0, black = 1) and the other bits represent the piece type.
    /// This lets us check a piece's type trivially via an &, but this is all hidden in an extension method, IsOfType.
    /// </summary>
    [Flags]
    public enum PieceType
    {
        White = 0,
        Black = 1,
        RealPiece = 2,
        King = 4,
        WhiteKing = King + RealPiece,
        BlackKing = King + RealPiece + Black,
        Queen = 8,
        WhiteQueen = Queen + RealPiece,
        BlackQueen = Queen + RealPiece + Black,
        Rook = 16,
        WhiteRook = Rook + RealPiece,
        BlackRook = Rook + RealPiece + Black,
        Bishop = 32,
        WhiteBishop = Bishop + RealPiece,
        BlackBishop = Bishop + RealPiece + Black,
        Knight = 64,
        WhiteKnight = Knight + RealPiece,
        BlackKnight = Knight + RealPiece + Black,
        Pawn = 128,
        WhitePawn = Pawn + RealPiece,
        BlackPawn = Pawn + RealPiece + Black,
    }
}