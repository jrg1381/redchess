using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using RedChess.ChessCommon.Enumerations;

namespace Redchess.Engine
{
    static class PieceData
    {
        private static readonly Dictionary<PieceType, IEnumerable<Location>> InitialConfigurations;
        private static readonly Dictionary<PieceType, string> SymbolLookup;
        private static readonly Dictionary<string, PieceType> ReverseSymbolLookup;

        private static readonly PieceType[] WhitePieceTypesSource =
        {
            PieceType.WhiteBishop, PieceType.WhiteKing, PieceType.WhiteKnight, PieceType.WhitePawn, PieceType.WhiteQueen, PieceType.WhiteRook
        };

        private static readonly PieceType[] BlackPieceTypesSource =
        {
            PieceType.BlackBishop, PieceType.BlackKing, PieceType.BlackKnight, PieceType.BlackPawn, PieceType.BlackQueen, PieceType.BlackRook
        };

        static PieceData()
        {
            InitialConfigurations = new Dictionary<PieceType, IEnumerable<Location>>();
            SymbolLookup = new Dictionary<PieceType, string>();
            ReverseSymbolLookup = new Dictionary<string, PieceType>();

            SymbolLookup[PieceType.NullPiece] = "X";
            InitialConfigurations[PieceType.NullPiece] = new Location[0];

            InitialConfigurations[PieceType.BlackPawn] = new[]
            {Location.A7, Location.B7, Location.C7, Location.D7, Location.E7, Location.F7, Location.G7, Location.H7};
            SymbolLookup[PieceType.BlackPawn] = "p";
            InitialConfigurations[PieceType.BlackBishop] = new[] { Location.C8, Location.F8 };
            SymbolLookup[PieceType.BlackBishop] = "b";
            InitialConfigurations[PieceType.BlackKnight] = new[] { Location.B8, Location.G8 };
            SymbolLookup[PieceType.BlackKnight] = "n";
            InitialConfigurations[PieceType.BlackRook] = new[] { Location.A8, Location.H8 };
            SymbolLookup[PieceType.BlackRook] = "r";
            InitialConfigurations[PieceType.BlackKing] = new[] { Location.E8 };
            SymbolLookup[PieceType.BlackKing] = "k";
            InitialConfigurations[PieceType.BlackQueen] = new[] { Location.D8 };
            SymbolLookup[PieceType.BlackQueen] = "q";

            InitialConfigurations[PieceType.WhitePawn] = new[]
            {Location.A2, Location.B2, Location.C2, Location.D2, Location.E2, Location.F2, Location.G2, Location.H2};
            SymbolLookup[PieceType.WhitePawn] = "P";
            InitialConfigurations[PieceType.WhiteKnight] = new[] { Location.B1, Location.G1 };
            SymbolLookup[PieceType.WhiteKnight] = "N";
            InitialConfigurations[PieceType.WhiteBishop] = new[] { Location.C1, Location.F1 };
            SymbolLookup[PieceType.WhiteBishop] = "B";
            InitialConfigurations[PieceType.WhiteRook] = new[] { Location.A1, Location.H1 };
            SymbolLookup[PieceType.WhiteRook] = "R";
            InitialConfigurations[PieceType.WhiteKing] = new[] { Location.E1 };
            SymbolLookup[PieceType.WhiteKing] = "K";
            InitialConfigurations[PieceType.WhiteQueen] = new[] { Location.D1 };
            SymbolLookup[PieceType.WhiteQueen] = "Q";

            foreach (KeyValuePair<PieceType, string> kvp in SymbolLookup)
            {
                ReverseSymbolLookup[kvp.Value] = kvp.Key;
            }
        }

        internal static IEnumerable<PieceType> WhitePieceTypes
        {
            get { return WhitePieceTypesSource.AsEnumerable(); }
        }

        internal static IEnumerable<PieceType> BlackPieceTypes
        {
            get { return BlackPieceTypesSource.AsEnumerable(); }
        }

        internal static IEnumerable<PieceType> AllPieceTypes
        {
            get { return WhitePieceTypes.Concat(BlackPieceTypes); }
        }

        internal static IEnumerable<Location> InitialPieceConfiguration(PieceType t)
        {
            IEnumerable<Location> retval;
            bool knownType = InitialConfigurations.TryGetValue(t, out retval);
            Debug.Assert(knownType, "Not a known type of piece!");
            return retval;
        }

        internal static string Symbol(PieceType pieceType)
        {
            string symbol;
            bool knownType = SymbolLookup.TryGetValue(pieceType, out symbol);
            Debug.Assert(knownType, "Not a known type of piece!");
            return symbol;
        }

        internal static PieceType TypeFromSymbol(char symbol)
        {
            return TypeFromSymbol(symbol.ToString(CultureInfo.InvariantCulture));
        }

        internal static PieceType TypeFromSymbol(string symbol)
        {
            PieceType typeOfSymbol;
            bool knownSymbol = ReverseSymbolLookup.TryGetValue(symbol, out typeOfSymbol);
            Debug.Assert(knownSymbol, "Not a known symbol");
            return typeOfSymbol;
        }
    }
}