using System;
using System.Collections.Generic;

namespace Redchess.Engine.Structures
{
    internal struct BitVector64
    {
        private UInt64 m_data;

        #region NotUsed

        /*	
        public BitVector64(UInt64 data)
        {
            m_data = data;
        }

		public BitVector64 Invert()
		{
			UInt64 v = m_data;
			v = (v & 0xaaaaaaaaaaaaaaaa) >> 1  | (v & 0x5555555555555555) << 1;
			v = (v & 0xcccccccccccccccc) >> 2  | (v & 0x3333333333333333) << 2;
			v = (v & 0xf0f0f0f0f0f0f0f0) >> 4  | (v & 0x0f0f0f0f0f0f0f0f) << 4;
			v = (v & 0xff00ff00ff00ff00) >> 8  | (v & 0x00ff00ff00ff00ff) << 8;
			v = (v & 0xffff0000ffff0000) >> 16 | (v & 0x0000ffff0000ffff) << 16;
			v = (v & 0xffffffff00000000) >> 32 | (v & 0x00000000ffffffff) << 32;
			return new BitVector64(v);
		}

		public BitVector64 Mirror()
		{
			UInt64 retval = 0;
			UInt64 v = m_data;
			UInt64 mask = 0x00000000000000ff;
			int shift = 0;

			while(mask != 0)
			{
				UInt64 tmp = (v & mask) >> shift;
				tmp = (tmp & 0xaa) >> 1 | (tmp & 0x55) << 1;
				tmp = (tmp & 0xcc) >> 2 | (tmp & 0x33) << 2;
				tmp = (tmp & 0xf0) >> 4 | (tmp & 0x0f) << 4;
				retval |= (tmp << shift);
				mask <<= 8;
				shift += 8;
			}

			return new BitVector64(retval);
		}

		public UInt64 Raw() { return m_data; }
		public static BitVector64 operator +(BitVector64 firstVector, BitVector64 secondVector) { return new BitVector64(firstVector.m_data | secondVector.m_data); }

		*/

        #endregion

        /// <summary>
        ///     Enumerates the set locations in the bit vector
        /// </summary>
        public IEnumerable<int> SetBits
        {
            get
            {
                UInt64 initialValue = m_data;
                int currentBit = 0;

                while (initialValue > 0)
                {
                    if ((initialValue & 1) == 1)
                    {
                        yield return currentBit;
                    }

                    initialValue >>= 1;
                    currentBit++;
                }
            }
        }

        public int CountSetBits()
        {
            UInt64 dataCopy = m_data;
            int ret = 0;

            while (dataCopy != 0)
            {
                dataCopy &= dataCopy - 1;
                ret++;
            }

            return ret;
        }

        public bool this[int bit]
        {
            get { return ((m_data >> bit) & 1) == 1; }
            set
            {
                if (value)
                {
                    m_data |= (ulong) 1 << bit;
                }
                else
                {
                    m_data &= ~((ulong) 1 << bit);
                }
            }
        }
    }
}