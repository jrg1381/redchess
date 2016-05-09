using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using RedChess.WebEngine.Repositories.Interfaces;

namespace RedChess.WebEngine.Models
{
    [Table("EloHistory")]
    class EloHistory : IEloHistory
    {
        [Key]
        public int EloHistoryId { get; set; }
        public DateTime Date { get; set; }
        public int UserId { get; set; }
        public int Elo { get; set; }
    }
}
