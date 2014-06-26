using System;
using System.Threading;

namespace WebTests
{
    public static class WaitHelper
    {
        /// <summary>
        /// Return true if wait was successful, false if it timed out
        /// </summary>
        /// <param name="timeout"></param>
        /// <param name="f"></param>
        /// <returns></returns>
        public static bool WaitFor(TimeSpan timeout, Func<bool> f)
        {
            DateTime expiryTime = DateTime.UtcNow.Add(timeout);

            while (!f() && DateTime.UtcNow < expiryTime)
            {
                Thread.Sleep(TimeSpan.FromMilliseconds(500));
            }

            return DateTime.UtcNow < expiryTime;
        }
    }
}