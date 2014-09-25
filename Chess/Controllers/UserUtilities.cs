using System.Linq;
using Chess.Models;

namespace Chess.Controllers
{
    public static class UserUtilities
    {
        public static UserProfile UserProfileFromName(ChessContext context)
        {
            if (System.Web.HttpContext.Current.Session["UserProfile"] == null)
            {
                var currentUserProfile = UserProfileFromName(context, System.Web.HttpContext.Current.User.Identity.Name);
                System.Web.HttpContext.Current.Session["UserProfile"] = currentUserProfile;
            }

            return System.Web.HttpContext.Current.Session["UserProfile"] as UserProfile;
        }

        public static UserProfile UserProfileFromName(ChessContext context, string name)
        {
            return context.UserProfiles.FirstOrDefault(x => x.UserName == name);
        }
    }
}