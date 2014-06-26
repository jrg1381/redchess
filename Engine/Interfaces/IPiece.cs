using System.Collections.Generic;
using RedChess.ChessCommon.Enumerations;
using Redchess.Engine.Structures;

namespace Redchess.Engine.Interfaces
{
    public interface IPiece
    {
        PieceColor Color { get; }
        PieceType Type { get; }
        Square Position { get; }
        IEnumerable<Location> ReachableSquares(IBoardExtended game);
        IEnumerable<Location> ValidMoves(IBoardExtended game);
        IEnumerable<Location> AttackedSquares(IBoardExtended game);
    }
}