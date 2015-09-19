namespace RedChess.MessageQueue
{
    public interface IQueueManager
    {
        void PostGameEndedMessage(int gameId);
        object PeekQueue();
        void PostRequestBestMoveMessage(int gameId, int moveNumber, string fen);
        void PostBestMoveResponseMessage(int gameId, int moveNumber, string bestMove);
    }
}