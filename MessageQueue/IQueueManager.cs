namespace RedChess.MessageQueue
{
    public interface IQueueManager
    {
        void PostGameEndedMessage(int gameId);
        object PeekQueue();
        void PostRequestBestMoveMessage(int gameId, int moveNumber, string fen, string move);
        void PostBestMoveResponseMessage(int gameId, int moveNumber, string bestMove, int boardEvaluation);
    }
}