namespace Chess.Repositories
{
    public interface IGameManager
    {
        IGameBinding FetchGame(int gameId);
    }
}