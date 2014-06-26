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
        private readonly char m_symbol;
        private readonly PieceType m_pieceType;

        internal BoardBitmap(PieceType pieceType, bool isEmpty = false) : this(new BitVector64(), pieceType)
        {
            if (!isEmpty)
            {
                foreach (var location in PieceData.InitialPieceConfiguration(pieceType))
                {
                    m_data[(int) location] = true;
                }
            }
        }

        private BoardBitmap(BitVector64 data, PieceType pieceType)
        {
            m_data = data;
            m_symbol = PieceData.Symbol(pieceType)[0];
            m_pieceType = pieceType;
        }

        private BoardBitmap(BoardBitmap source)
        {
            m_data = source.m_data;
            m_pieceType = source.m_pieceType;
            m_symbol = source.m_symbol;
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