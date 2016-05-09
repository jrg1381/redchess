using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using RedChess.ChessCommon.Enumerations;

namespace Redchess.Engine
{
    static class PieceData
    {
        private static readonly Dictionary<PieceType, IEnumerable<Location>> s_InitialConfigurations;
        private static readonly Dictionary<PieceType, string> s_SymbolLookup;
        private static readonly Dictionary<string, PieceType> s_ReverseSymbolLookup;

        private static readonly PieceType[] s_WhitePieceTypesSource =
        {
            PieceType.WhiteBishop, PieceType.WhiteKing, PieceType.WhiteKnight, PieceType.WhitePawn, PieceType.WhiteQueen, PieceType.WhiteRook
        };

        private static readonly PieceType[] s_BlackPieceTypesSource =
        {
            PieceType.BlackBishop, PieceType.BlackKing, PieceType.BlackKnight, PieceType.BlackPawn, PieceType.BlackQueen, PieceType.BlackRook
        };

        static PieceData()
        {
            s_InitialConfigurations = new Dictionary<PieceType, IEnumerable<Location>>();
            s_SymbolLookup = new Dictionary<PieceType, string>();
            s_ReverseSymbolLookup = new Dictionary<string, PieceType>();

            s_InitialConfigurations[PieceType.BlackPawn] = new[]
            {Location.A7, Location.B7, Location.C7, Location.D7, Location.E7, Location.F7, Location.G7, Location.H7};
            s_SymbolLookup[PieceType.BlackPawn] = "p";
            s_InitialConfigurations[PieceType.BlackBishop] = new[] { Location.C8, Location.F8 };
            s_SymbolLookup[PieceType.BlackBishop] = "b";
            s_InitialConfigurations[PieceType.BlackKnight] = new[] { Location.B8, Location.G8 };
            s_SymbolLookup[PieceType.BlackKnight] = "n";
            s_InitialConfigurations[PieceType.BlackRook] = new[] { Location.A8, Location.H8 };
            s_SymbolLookup[PieceType.BlackRook] = "r";
            s_InitialConfigurations[PieceType.BlackKing] = new[] { Location.E8 };
            s_SymbolLookup[PieceType.BlackKing] = "k";
            s_InitialConfigurations[PieceType.BlackQueen] = new[] { Location.D8 };
            s_SymbolLookup[PieceType.BlackQueen] = "q";

            s_InitialConfigurations[PieceType.WhitePawn] = new[]
            {Location.A2, Location.B2, Location.C2, Location.D2, Location.E2, Location.F2, Location.G2, Location.H2};
            s_SymbolLookup[PieceType.WhitePawn] = "P";
            s_InitialConfigurations[PieceType.WhiteKnight] = new[] { Location.B1, Location.G1 };
            s_SymbolLookup[PieceType.WhiteKnight] = "N";
            s_InitialConfigurations[PieceType.WhiteBishop] = new[] { Location.C1, Location.F1 };
            s_SymbolLookup[PieceType.WhiteBishop] = "B";
            s_InitialConfigurations[PieceType.WhiteRook] = new[] { Location.A1, Location.H1 };
            s_SymbolLookup[PieceType.WhiteRook] = "R";
            s_InitialConfigurations[PieceType.WhiteKing] = new[] { Location.E1 };
            s_SymbolLookup[PieceType.WhiteKing] = "K";
            s_InitialConfigurations[PieceType.WhiteQueen] = new[] { Location.D1 };
            s_SymbolLookup[PieceType.WhiteQueen] = "Q";

            foreach (KeyValuePair<PieceType, string> kvp in s_SymbolLookup)
            {
                s_ReverseSymbolLookup[kvp.Value] = kvp.Key;
            }
        }

        internal static IEnumerable<PieceType> WhitePieceTypes => s_WhitePieceTypesSource.AsEnumerable();

        internal static IEnumerable<PieceType> BlackPieceTypes => s_BlackPieceTypesSource.AsEnumerable();

        internal static IEnumerable<Location> InitialPieceConfiguration(PieceType t)
        {
            IEnumerable<Location> retval;
            bool knownType = s_InitialConfigurations.TryGetValue(t, out retval);
            Debug.Assert(knownType, "Not a known type of piece!");
            return retval;
        }

        internal static string Symbol(PieceType pieceType)
        {
            string symbol;
            bool knownType = s_SymbolLookup.TryGetValue(pieceType, out symbol);
            Debug.Assert(knownType, "Not a known type of piece!");
            return symbol;
        }

        internal static PieceType TypeFromSymbol(char symbol)
        {
            return TypeFromSymbol(symbol.ToString(CultureInfo.InvariantCulture));
        }

        static PieceType TypeFromSymbol(string symbol)
        {
            PieceType typeOfSymbol;
            bool knownSymbol = s_ReverseSymbolLookup.TryGetValue(symbol, out typeOfSymbol);
            Debug.Assert(knownSymbol, "Not a known symbol");
            return typeOfSymbol;
        }
    }
}