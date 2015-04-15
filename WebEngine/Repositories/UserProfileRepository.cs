using System.Collections.Generic;
using System.Linq;
using RedChess.WebEngine.Models;

namespace RedChess.WebEngine.Repositories
{
    public class UserProfileRepository
    {
        public IEnumerable<UserProfile> FindAll()
        {
            using (var context = new ChessContext())
            {
                return context.UserProfiles.ToList();
            }
        }

        internal int UserId(string currentUser)
        {
            using (var context = new ChessContext())
            {
                return context.UserProfiles.Single(u => u.UserName == currentUser).UserId;
            }
        }
    }
}