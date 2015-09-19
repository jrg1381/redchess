using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace RedChess.WebEngine.Models
{
    [Table("Analysis")]
    public class AnalysisEntry
    {
        [Key]
        public int GameId { get; set; }

        public int MoveNumber { get; set; }
        public string Analysis { get; set; }
    }
}