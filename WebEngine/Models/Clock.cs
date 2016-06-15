using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using RedChess.WebEngine.Repositories;

namespace RedChess.WebEngine.Models
{
    [Table("Clocks")]
    public class Clock : IClock
    {
		public Clock()
		{
		}

		public Clock(int id, int timeLimitMs = 0)
		{
			GameId = id;
			LastActionBlack = DateTime.UtcNow;
			LastActionWhite = DateTime.UtcNow;
			TimeElapsedBlackMs = 0;
			TimeElapsedWhiteMs = 0;
			TimeLimitMs = timeLimitMs;
			PlayersReady = 0;
		}

		[Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int ClockId { get; set; }
        public int GameId { get; set; }
		public DateTime LastActionBlack { get; set; }
		public DateTime LastActionWhite { get; set; }
		public int TimeElapsedBlackMs { get; set; }
		public int TimeElapsedWhiteMs { get; set; }
		public int TimeLimitMs { get; set; }
		public PlayerReadyStatus PlayersReady { get; set; }
        public bool IsTimedGame => TimeLimitMs != 0;
    }
}