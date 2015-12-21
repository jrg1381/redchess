using System;
using RedChess.ChessCommon.Enumerations;

namespace RedChess.WebTests
{
    class Offset
    {
        // Could pull these in from inspection of the DOM
        private const int BoardSize = 512;
        private const int SquareSize = BoardSize/8;
        private const int SymbolOffset = SquareSize/2;

        public int X { get; private set; }
        public int Y { get; private set; }

        public Offset(Location location, PieceColor turnColor)
        {
            var loc = (int) location;
            switch (turnColor)
            {
                case PieceColor.Black:
                    X = (7 - loc%8)*SquareSize + SymbolOffset;
                    Y = BoardSize - (7 - loc/8)*SquareSize - SymbolOffset;
                    break;
                case PieceColor.White:
                    X = (loc%8)*SquareSize + SymbolOffset;
                    Y = BoardSize - (loc/8)*SquareSize - SymbolOffset;
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(turnColor));
            }
        }
    }
}