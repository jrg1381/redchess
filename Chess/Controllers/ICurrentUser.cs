namespace Chess.Controllers
{
    public interface ICurrentUser
    {
        string CurrentUser { get; }
        int CurrentUserId { get; }
    }
}