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
    }
}