using System.Collections.Generic;

namespace RedChess.ChessCommon
{
    public class MruCache<TK,TV>
    {
        private readonly Dictionary<TK, TV> m_Data;
        private readonly TK[] m_OrderedKeys;
        private int m_Index;
        private readonly int m_Size;

        public MruCache(int size)
        {
            m_Data = new Dictionary<TK, TV>(size);
            m_OrderedKeys = new TK[size];
            m_Size = size;
            m_Index = 0;
        }

        public void Set(TK key, TV value)
        {
            if (m_OrderedKeys[m_Index] != null)
                m_Data.Remove(m_OrderedKeys[m_Index]);
            m_Data[key] = value;
            m_OrderedKeys[m_Index] = key;
            m_Index = (m_Index + 1) % m_Size;
        }

        public bool TryGet(TK key, out TV value)
        {
            return m_Data.TryGetValue(key, out value);
        }
    }
}