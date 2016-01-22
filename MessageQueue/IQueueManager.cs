using RedChess.ChessCommon;
using RedChess.ChessCommon.Interfaces;

namespace RedChess.MessageQueue
{
    public interface IQueueManager
    {
        void PostGameEndedMessage(int gameId);
        void PostRequestBestMoveMessage(int gameId, int moveNumber, string fen, string move);
        void PostBestMoveResponseMessage(int gameId, int moveNumber, IBoardAnalysis bestMove);
        long QueryQueueLength();
    }
}