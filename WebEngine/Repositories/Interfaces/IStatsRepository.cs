using System.Collections;
using System.Collections.Generic;

namespace RedChess.WebEngine.Repositories.Interfaces
{
    internal interface IStatsRepository
    {
        IEnumerable<IStats> Stats();
    }
}