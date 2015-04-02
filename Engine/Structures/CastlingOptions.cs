using System;

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
}