namespace RedChess.MessageQueue
{
    public interface IQueueManager
    {
        void PostGameEndedMessage(int gameId, string pgnText);
        object PeekQueue();
    }
}