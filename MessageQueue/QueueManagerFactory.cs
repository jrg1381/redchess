using System;
using System.Diagnostics;
using Microsoft.Azure;

namespace RedChess.MessageQueue
{
    public static class QueueManagerFactory
    {
        public static IQueueManager CreateInstance()
        {
            var connectionString = CloudConfigurationManager.GetSetting("Microsoft.ServiceBus.ConnectionString");
            if (String.IsNullOrEmpty(connectionString)) return new NullQueue();
            return new QueueManager(connectionString);
        }
    }

    internal class NullQueue : IQueueManager
    {
        public void PostGameEndedMessage(int gameId, string pgnText)
        {
            Debug.WriteLine("{0} {1}", gameId, pgnText);
        }

        public object PeekQueue()
        {
            return new object();
        }
    }
}