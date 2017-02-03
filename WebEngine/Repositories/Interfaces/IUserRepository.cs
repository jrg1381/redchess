using System.Collections.Generic;
using RedChess.WebEngine.Models;

namespace RedChess.WebEngine.Repositories.Interfaces
{
    interface IUserProfileRepository
    {
        IEnumerable<UserProfile> FindAll();
        int UserId(string currentUser);
        string GetEmailHashForUsername(string username);
        void SetEmailHashForUserId(int userId, string emailHash);
        bool IsAdministrator(int userId);
    }
}
