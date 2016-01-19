using System.Collections.Generic;
using System.Linq;
using RedChess.WebEngine.Models;
using RedChess.WebEngine.Repositories.Interfaces;

namespace RedChess.WebEngine.Repositories
{
    internal class UserProfileRepository : IUserProfileRepository
    {
        public IEnumerable<UserProfile> FindAll()
        {
            using (var context = new ChessContext())
            {
                return context.UserProfiles.ToList();
            }
        }

        public int UserId(string currentUser)
        {
            using (var context = new ChessContext())
            {
                return context.UserProfiles.Single(u => u.UserName == currentUser).UserId;
            }
        }

        public string GetEmailHashForUsername(string username)
        {
            using (var context = new ChessContext())
            {
                return context.UserProfiles.FirstOrDefault(u => u.UserName == username)?.EmailHash;
            }
        }

        public void SetEmailHashForUserId(int userId, string emailHash)
        {
            using (var context = new ChessContext())
            {
                context.UserProfiles.First(u => u.UserId == userId).EmailHash = emailHash;
                context.SaveChanges();
            }
        }
    }
}