using System;

namespace RedChess.ChessCommon.Enumerations
{
    /// <summary>
    /// The idea here is that the lowest bits represents the colour (white = 1, black = 2) and the other bits represent the piece type.
    /// This lets us check a piece's type trivially via an &, but this is all hidden in an extension method, IsOfType.
    /// </summary>
    [Flags]
    public enum PieceType
    {
        None = 0,
        White = 1,
        Black = 2,
        King = 4,
        WhiteKing = King + White,
        BlackKing = King + Black,
        Queen = 8,
        WhiteQueen = Queen + White,
        BlackQueen = Queen + Black,
        Rook = 16,
        WhiteRook = Rook + White,
        BlackRook = Rook + Black,
        Bishop = 32,
        WhiteBishop = Bishop + White,
        BlackBishop = Bishop + Black,
        Knight = 64,
        WhiteKnight = Knight + White,
        BlackKnight = Knight + Black,
        Pawn = 128,
        WhitePawn = Pawn + White,
        BlackPawn = Pawn + Black,
    }
}