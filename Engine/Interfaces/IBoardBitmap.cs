using System.Collections.Generic;
using RedChess.ChessCommon.Enumerations;

namespace Redchess.Engine.Interfaces
{
    public interface IBoardBitmap
    {
        IEnumerable<Location> OccupiedSquares();
        void Add(Location loc);
        void Remove(Location loc);
        bool IsOccupied(Location loc);
        IBoardBitmap DeepClone();
    }
}