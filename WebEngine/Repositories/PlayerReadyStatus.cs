using System;

namespace RedChess.WebEngine.Repositories
{
    [Flags]
    public enum PlayerReadyStatus
    {
        Nobody = 0,
        Black = 1,
        White = 2,
        Both = 3
    }
}