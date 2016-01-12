using System.Collections.Generic;
using RedChess.WebEngine.Models;

namespace RedChess.WebEngine.Repositories.Interfaces
{
    interface IUserProfileRepository
    {
        IEnumerable<UserProfile> FindAll();
        int UserId(string currentUser);
        string EmailHashForUsername(string username);
    }
}
