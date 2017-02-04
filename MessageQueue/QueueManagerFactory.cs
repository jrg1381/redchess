using System;
using System.Diagnostics;
using Microsoft.Azure;
using RedChess.ChessCommon;
using RedChess.ChessCommon.Interfaces;

namespace RedChess.MessageQueue
{
    public static class QueueManagerFactory
    {
        public const string QueueName = "engine";

        private static readonly object Lock = new object();
        private static IQueueManager s_QueueManager;

        public static IQueueManager CreateInstance()
        {
#if DEBUG
            return new NullQueue();
#endif
            lock (Lock)
            {
                if (s_QueueManager == null)
                {
                    var connectionString = CloudConfigurationManager.GetSetting("Microsoft.ServiceBus.ConnectionString");
                    if (String.IsNullOrEmpty(connectionString)) return new NullQueue();
                    s_QueueManager = new QueueManager(connectionString);
                }
            }

            return s_QueueManager;
        }
    }

    internal class NullQueue : IQueueManager
    {
        public void PostGameEndedMessage(int gameId)
        {
        }

        public void PostRequestBestMoveMessage(int gameId, int moveId, string fen, string move)
        {        
        }

        public void PostBestMoveResponseMessage(int gameId, int moveNumber, IBoardAnalysis foo)
        {
        }

        public long QueryQueueLength()
        {
            return 0;
        }
    }
}