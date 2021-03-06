using System;
using System.Collections.Generic;

namespace RedChess.WebEngine.Repositories.Interfaces
{
    internal interface IStatsRepository
    {
        IEnumerable<IStats> Stats();
        IEnumerable<IEloHistory> Elo();
        void UpdateEloTable();
        DateTime LastUpdate();
    }
}