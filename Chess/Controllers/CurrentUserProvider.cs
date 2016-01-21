using WebMatrix.WebData;

namespace Chess.Controllers
{
    public class CurrentUserProvider : ICurrentUser
    {
        public string CurrentUser => System.Web.HttpContext.Current.User.Identity.Name.ToLowerInvariant();
        public int CurrentUserId => WebSecurity.CurrentUserId;
    }
}