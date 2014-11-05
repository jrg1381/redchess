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
        private int m_setBits;

        internal BoardBitmap()
        {
            m_data = new BitVector64();
            m_setBits = 0;
        }

        private BoardBitmap(BoardBitmap source)
        {
            m_data = source.m_data;
            m_setBits = m_data.CountSetBits();
        }

        /// <summary>
        ///     Removes a piece. 
        /// </summary>
        /// <param name="loc"></param>
        /// <returns></returns>
        public void Remove(Location loc)
        {
            m_data[(int) loc] = false;
            m_setBits--;
        }

        public void Add(Location loc)
        {
            m_data[(int) loc] = true;
            m_setBits++;
        }

        public int PieceCount
        {
            get { return m_setBits; }
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