using System.Collections.Generic;

namespace RedChess.ChessCommon
{
    public class LockFactory
    {
        private static readonly object s_GlobalLock = new object();
        static readonly Dictionary<int, object> s_locks = new Dictionary<int, object>(); 

        public static object GetLock(int id)
        {
            lock (s_GlobalLock)
            {
                object retval;
                if (!s_locks.TryGetValue(id, out retval))
                {
                    retval = new object();
                    s_locks.Add(id, retval);

                }
                return retval;
            }
        }

        public static void Purge(int id)
        {
            lock (s_GlobalLock)
            {
                s_locks.Remove(id);
            }
        }
    }
}