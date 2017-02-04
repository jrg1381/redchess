using System;
using RedChess.ChessCommon.Enumerations;

namespace RedChess.WebTests
{
    class Offset
    {
        // Could pull these in from inspection of the DOM
        private const int c_BoardSize = 512;
        private const int c_SquareSize = c_BoardSize/8;
        private const int c_SymbolOffset = c_SquareSize/2;

        public int X { get; private set; }
        public int Y { get; private set; }

        public Offset(Location location, PieceColor turnColor)
        {
            var loc = (int) location;
            switch (turnColor)
            {
                case PieceColor.Black:
                    X = (7 - loc%8)*c_SquareSize + c_SymbolOffset;
                    Y = c_BoardSize - (7 - loc/8)*c_SquareSize - c_SymbolOffset;
                    break;
                case PieceColor.White:
                    X = (loc%8)*c_SquareSize + c_SymbolOffset;
                    Y = c_BoardSize - (loc/8)*c_SquareSize - c_SymbolOffset;
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(turnColor));
            }
        }
    }
}