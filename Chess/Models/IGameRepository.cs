namespace Chess.Models
{
    public interface IGameRepository
    {
        IGame FindById(int id);
    }
}