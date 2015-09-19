using System;
using System.Diagnostics;
using Microsoft.Azure;

namespace RedChess.MessageQueue
{
    public static class QueueManagerFactory
    {
        public const string QueueName = "engine";

        public static IQueueManager CreateInstance()
        {
            var connectionString = CloudConfigurationManager.GetSetting("Microsoft.ServiceBus.ConnectionString");
            if (String.IsNullOrEmpty(connectionString)) return new NullQueue();
            return new QueueManager(connectionString);
        }
    }

    internal class NullQueue : IQueueManager
    {
        public void PostGameEndedMessage(int gameId)
        {
        }

        public object PeekQueue()
        {
            return new object();
        }

        public void PostRequestBestMoveMessage(int gameId, int moveId, string fen)
        {        
        }

        public void PostBestMoveResponseMessage(int gameId, int moveNumber, string bestMove)
        {
        }
    }
}