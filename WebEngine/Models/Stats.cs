using System.ComponentModel.DataAnnotations;
using RedChess.WebEngine.Repositories.Interfaces;

namespace RedChess.WebEngine.Models
{
    public class Stats : IStats
    {
        [Key]
        public string White { get; set; }
        public string Black { get; set; }
        public string Winner { get; set; }
        public int Count { get; set; }
    }
}