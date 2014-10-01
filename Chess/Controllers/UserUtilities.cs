using System.Collections.Generic;
using System.Linq;
using Chess.Models;

namespace Chess.Controllers
{
    public static class UserUtilities
    {
        private static readonly Dictionary<int, UserProfile> s_userProfilesById = new Dictionary<int, UserProfile>(); 
        private static readonly Dictionary<string, UserProfile> s_userProfilesByName = new Dictionary<string, UserProfile>(); 

        public static UserProfile UserProfileFromName(ChessContext context)
        {
            return UserProfileFromName(context, System.Web.HttpContext.Current.User.Identity.Name);
        }

        public static UserProfile UserProfileFromName(string name)
        {
            UserProfile profile;

            if (!s_userProfilesByName.TryGetValue(name, out profile))
            {
                using (var context = new ChessContext())
                {
                    return UserProfileFromName(context, name);
                }
            }

            return profile;
        }

        public static UserProfile UserProfileFromId(int id)
        {
            UserProfile profile;

            if (!s_userProfilesById.TryGetValue(id, out profile))
            {
                using (var context = new ChessContext())
                {
                    return UserProfileFromId(context, id);
                }
            }

            return profile;
        }

        public static UserProfile UserProfileFromName(ChessContext context, string name)
        {
            UserProfile profile;

            if (!s_userProfilesByName.TryGetValue(name, out profile))
            {
                profile = context.UserProfiles.FirstOrDefault(x => x.UserName == name);
                s_userProfilesByName[name] = profile;
            }

            return profile;
        }

        public static UserProfile UserProfileFromId(ChessContext context, int userId)
        {
            UserProfile profile;

            if (!s_userProfilesById.TryGetValue(userId, out profile))
            {
                profile = context.UserProfiles.FirstOrDefault(x => x.UserId == userId);
                s_userProfilesById[userId] = profile;
            }

            return profile;
        }
    }
}