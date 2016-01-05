namespace Chess.Controllers
{
    public class CurrentUserImpl : ICurrentUser
    {
        public string CurrentUser => System.Web.HttpContext.Current.User.Identity.Name.ToLowerInvariant();
    }
}