using System;
using System.Collections.Generic;
using RedChess.ChessCommon.Interfaces;

namespace RedChess.ChessCommon
{
    class HistoryEntryEqualityComparer : IEqualityComparer<IHistoryEntry>
    {
        public bool Equals(IHistoryEntry x, IHistoryEntry y)
        {
            return x.Move == y.Move && x.Fen == y.Fen && x.MoveNumber == y.MoveNumber;
        }

        public int GetHashCode(IHistoryEntry obj)
        {
            throw new NotImplementedException();
        }
    }
}