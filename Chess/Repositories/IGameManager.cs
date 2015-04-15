using RedChess.ChessCommon.Interfaces;

namespace Chess.Repositories
{
    public interface IGameManager
    {
        IGameBinding FetchGame(int gameId);
        int Add(IBoard board, int opponentId, string currentUser, bool playAsBlack, int timeLimitMs);
    }
}