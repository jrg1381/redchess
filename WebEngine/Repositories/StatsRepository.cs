using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Azure;
using RedChess.WebEngine.Repositories.Interfaces;

namespace RedChess.WebEngine.Repositories
{
    internal class StatsRepository : IStatsRepository
    {
        public IEnumerable<IStats> Stats()
        {
            using (var context = new ChessContext())
            {
                return context.Stats.SqlQuery("EXEC dbo.GameStatistics").AsNoTracking().ToList();
            }
        }

        public IEnumerable<IEloHistory> Elo()
        {
            using (var context = new ChessContext())
            {
                return context.EloEntries.AsNoTracking().ToList();
            }
        }

        public void UpdateEloTable()
        {
            var connectionString = CloudConfigurationManager.GetSetting("DefaultConnection");
            using (var context = new ChessContext(connectionString))
            {
                context.Database.ExecuteSqlCommand("EXEC dbo.UpdateEloTable");
            }
        }

        public DateTime LastUpdate()
        {
            const string query = "select [Value] from dbo.Metadata where [Key] = 'LastEloHistoryUpdate'";

            using (var context = new ChessContext())
            {
                var dateAsString = context.Database.SqlQuery<string>(query).FirstOrDefault();
                if (dateAsString == null)
                    return DateTime.MinValue;
                return DateTime.Parse(dateAsString);
            }
        }
    }
}