using System.Collections.Generic;
using System.Linq;
using RedChess.ChessCommon.Enumerations;
using Redchess.Engine.Interfaces;
using Redchess.Engine.Structures;

namespace Redchess.Engine
{
    internal sealed class BoardBitmap : IBoardBitmap
    {
        private BitVector64 m_data;

        internal BoardBitmap()
        {
            m_data = new BitVector64();
        }

        private BoardBitmap(BoardBitmap source)
        {
            m_data = source.m_data;
        }

        /// <summary>
        ///     Removes a piece. 
        /// </summary>
        /// <param name="loc"></param>
        /// <returns></returns>
        public void Remove(Location loc)
        {
            m_data[(int) loc] = false;
        }

        public void Add(Location loc)
        {
            m_data[(int) loc] = true;
        }

        public IEnumerable<Location> OccupiedSquares()
        {
            return m_data.SetBits.Cast<Location>();
        }

        public bool IsOccupied(Location loc)
        {
            return m_data[(int) loc];
        }

        public IBoardBitmap DeepClone()
        {
            return new BoardBitmap(this);
        }
    }
}