using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;

namespace Chess.Models
{
	public class ChessContext : DbContext
	{
        // You can add custom code to this file. Changes will not be overwritten.
        // 
        // If you want Entity Framework to drop and regenerate your database
        // automatically whenever you change your model schema, add the following
        // code to the Application_Start method in your Global.asax file.
        // Note: this will destroy and re-create your database with every model change.
        // 
        // System.Data.Entity.Database.SetInitializer(new System.Data.Entity.DropCreateDatabaseIfModelChanges<Chess.Models.ChessContext>());

	    private static Dictionary<string, int> s_playerNameToId; 

        public ChessContext() : base("name=DefaultConnection")
        {
            s_playerNameToId = new Dictionary<string, int>();
        }

		public DbSet<BoardDto> Boards { get; set; }
		public DbSet<Clock> Clocks { get; set; }
        public DbSet<HistoryEntry> HistoryEntries { get; set; }
        public DbSet<UserProfile> UserProfiles { get; set; }

        public int PlayerId(string playerName)
        {
            // Out parameter
            int answer;

            // There is a cached value
            if (s_playerNameToId.TryGetValue(playerName, out answer))
                return answer;

            var query = from user in UserProfiles where user.UserName == playerName select user.UserId;
            answer = query.SingleOrDefault();

            if (answer == 0) return -1;

            s_playerNameToId.Add(playerName, answer);
            return answer;
        }
	}
}
