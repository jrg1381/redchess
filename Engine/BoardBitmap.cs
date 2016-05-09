using System.Collections.Generic;
using System.Linq;
using RedChess.ChessCommon.Enumerations;
using Redchess.Engine.Interfaces;
using Redchess.Engine.Structures;

namespace Redchess.Engine
{
    sealed class BoardBitmap : IBoardBitmap
    {
        private BitVector64 m_Data;
        private int m_SetBits;

        internal BoardBitmap()
        {
            m_Data = new BitVector64();
            m_SetBits = 0;
        }

        private BoardBitmap(BoardBitmap source)
        {
            m_Data = source.m_Data;
            m_SetBits = m_Data.CountSetBits();
        }

        /// <summary>
        ///     Removes a piece. 
        /// </summary>
        /// <param name="loc"></param>
        /// <returns></returns>
        public void Remove(Location loc)
        {
            m_Data[(int) loc] = false;
            m_SetBits--;
        }

        public void Add(Location loc)
        {
            m_Data[(int) loc] = true;
            m_SetBits++;
        }

        public int PieceCount => m_SetBits;

        public IEnumerable<Location> OccupiedSquares()
        {
            return m_Data.SetBits.Cast<Location>();
        }

        public bool IsOccupied(Location loc)
        {
            return m_Data[(int) loc];
        }

        public IBoardBitmap DeepClone()
        {
            return new BoardBitmap(this);
        }
    }
}