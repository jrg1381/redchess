using System;
using System.ComponentModel.DataAnnotations;
using RedChess.WebEngine.Repositories;

namespace RedChess.WebEngine.Models
{
    public class Clock : IClock
    {
		public static readonly DateTime MinimumDateTime;

		static Clock()
		{
			MinimumDateTime = new DateTime(1900, 1, 1);
		}

		public Clock()
		{			
		}

		public Clock(int id, int timeLimitMs)
		{
			GameId = id;
			LastActionBlack = MinimumDateTime;
			LastActionWhite = MinimumDateTime;
			TimeElapsedBlackMs = 0;
			TimeElapsedWhiteMs = 0;
			TimeLimitMs = timeLimitMs;
			PlayersReady = 0;
		}

		[Key]
		public int ClockId { get; set; }
		public int GameId { get; set; }
		public DateTime LastActionBlack { get; set; }
		public DateTime LastActionWhite { get; set; }
		public int TimeElapsedBlackMs { get; set; }
		public int TimeElapsedWhiteMs { get; set; }
		public int TimeLimitMs { get; set; }
		public PlayerReadyStatus PlayersReady { get; set; }
	}
}