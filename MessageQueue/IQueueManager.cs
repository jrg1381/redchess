namespace RedChess.MessageQueue
{
    public interface IQueueManager
    {
        void PostGameEndedMessage(int gameId, string pgnText);
        object PeekQueue();
        void PostRequestBestMoveMessage(int gameId, string fen);
        void PostBestMoveResponseMessage(int gameId, string bestMove);
    }
}