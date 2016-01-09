using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
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
