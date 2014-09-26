using System.Collections.Generic;
using System.Linq;
using Chess.Models;

namespace Chess.Controllers
{
    public static class UserUtilities
    {
        private static readonly Dictionary<string, UserProfile> s_userProfiles = new Dictionary<string, UserProfile>(); 

        public static UserProfile UserProfileFromName(ChessContext context)
        {
            return UserProfileFromName(context, System.Web.HttpContext.Current.User.Identity.Name);
        }

        public static UserProfile UserProfileFromName(ChessContext context, string name)
        {
            UserProfile profile;

            if (!s_userProfiles.TryGetValue(name, out profile))
            {
                profile = context.UserProfiles.FirstOrDefault(x => x.UserName == name);
                s_userProfiles[name] = profile;
            }

            return profile;
        }
    }
}