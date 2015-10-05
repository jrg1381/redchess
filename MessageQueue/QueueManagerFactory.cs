using System;
using System.Diagnostics;
using Microsoft.Azure;
using RedChess.ChessCommon.Interfaces;

namespace RedChess.MessageQueue
{
    public static class QueueManagerFactory
    {
        public const string QueueName = "engine";

        private static readonly object s_lock = new object();
        private static IQueueManager s_queueManager;

        public static IQueueManager CreateInstance()
        {
#if DEBUG
            return new NullQueue();
#endif
            lock (s_lock)
            {
                if (s_queueManager == null)
                {
                    var connectionString = CloudConfigurationManager.GetSetting("Microsoft.ServiceBus.ConnectionString");
                    if (String.IsNullOrEmpty(connectionString)) return new NullQueue();
                    s_queueManager = new QueueManager(connectionString);
                }
            }

            return s_queueManager;
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

        public void PostRequestBestMoveMessage(int gameId, int moveId, string fen, string move)
        {        
        }

        public void PostBestMoveResponseMessage(int gameId, int moveNumber, IWorkItemResponse foo)
        {
        }
    }
}