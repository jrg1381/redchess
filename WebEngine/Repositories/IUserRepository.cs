using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RedChess.WebEngine.Models;

namespace RedChess.WebEngine.Repositories
{
    interface IUserProfileRepository
    {
        IEnumerable<UserProfile> FindAll();
        int UserId(string currentUser);
    }
}
