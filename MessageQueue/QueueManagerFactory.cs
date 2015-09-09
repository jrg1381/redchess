namespace RedChess.MessageQueue
{
    public static class QueueManagerFactory
    {
        public static IQueueManager Instance()
        {
            return new QueueManager();
        }
    }
}