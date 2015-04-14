using System;

namespace Chess.Models
{
    public interface IClock
    {
        int ClockId { get; set; }
        int GameId { get; set; }
        DateTime LastActionBlack { get; set; }
        DateTime LastActionWhite { get; set; }
        int TimeElapsedBlackMs { get; set; }
        int TimeElapsedWhiteMs { get; set; }
        int TimeLimitMs { get; set; }
        int PlayersReady { get; set; }
    }
}