using System.Collections.Generic;

namespace RedChess.ChessCommon
{
    public class MruCache<TK,TV>
    {
        private readonly Dictionary<TK, TV> m_data;
        private readonly TK[] m_orderedKeys;
        private int m_index = 0;
        private readonly int m_size;

        public MruCache(int size)
        {
            m_data = new Dictionary<TK, TV>(size);
            m_orderedKeys = new TK[size];
            m_size = size;
        }

        public void Set(TK key, TV value)
        {
            if (m_orderedKeys[m_index] != null)
                m_data.Remove(m_orderedKeys[m_index]);
            m_data[key] = value;
            m_orderedKeys[m_index] = key;
            m_index = (m_index + 1) % m_size;
        }

        public bool TryGet(TK key, out TV value)
        {
            return m_data.TryGetValue(key, out value);
        }
    }
}