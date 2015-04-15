﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Chess.Models;
using WebGrease.Css.Extensions;

namespace Chess.Repositories
{
    public class UserProfileRepository
    {
        public IEnumerable<UserProfile> FindAll()
        {
            using (var context = new ChessContext())
            {
                return context.UserProfiles.ToSafeReadOnlyCollection();
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