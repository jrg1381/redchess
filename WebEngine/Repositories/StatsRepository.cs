using System.Collections.Generic;
using System.Linq;
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
            using (var context = new ChessContext())
            {
                context.Database.ExecuteSqlCommand("EXEC dbo.UpdateEloTable");
            }
        }
    }
}