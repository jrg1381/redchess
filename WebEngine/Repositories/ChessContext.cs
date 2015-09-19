using System.Data.Entity;
using RedChess.WebEngine.Models;

namespace RedChess.WebEngine.Repositories
{
	internal class ChessContext : DbContext
	{
        // You can add custom code to this file. Changes will not be overwritten.
        // 
        // If you want Entity Framework to drop and regenerate your database
        // automatically whenever you change your model schema, add the following
        // code to the Application_Start method in your Global.asax file.
        // Note: this will destroy and re-create your database with every model change.
        // 
        // System.Data.Entity.Database.SetInitializer(new System.Data.Entity.DropCreateDatabaseIfModelChanges<Chess.Models.ChessContext>());

	    public ChessContext(string connectionString) : base(connectionString)
	    {
	    }

        public ChessContext() : base("name=DefaultConnection")
        {
        }

		public DbSet<GameDto> Boards { get; set; }
		public DbSet<Clock> Clocks { get; set; }
        public DbSet<HistoryEntry> HistoryEntries { get; set; }
        public DbSet<UserProfile> UserProfiles { get; set; }
        public DbSet<AnalysisEntry> AnalysisEntries { get; set; }
	}
}
