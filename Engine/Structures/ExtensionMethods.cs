using System;

namespace Redchess.Engine.Structures
{
    public static class ExtensionMethods
    {
        public static string Fen(this CastlingOptions castlingOptions)
        {
            if (castlingOptions == CastlingOptions.None)
                return "-";

            var whiteKingside = castlingOptions.HasFlag(CastlingOptions.WhiteKingSide) ? "K" : "";
            var whiteQueenside = castlingOptions.HasFlag(CastlingOptions.WhiteQueenSide) ? "Q" : "";
            var blackKingside = castlingOptions.HasFlag(CastlingOptions.BlackKingSide) ? "k" : "";
            var blackQueenside = castlingOptions.HasFlag(CastlingOptions.BlackQueenSide) ? "q" : "";

            return String.Format("{0}{1}{2}{3}", whiteKingside, whiteQueenside, blackKingside, blackQueenside);
        }
    }
}