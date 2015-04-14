using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using Chess.Controllers;
using Chess.Repositories;

namespace Chess.Models
{
    public class HistoryEntry
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int HistoryId { get; set; }
        public int GameId { get; set; }
        public int MoveNumber { get; set; }
        public string Fen { get; set; }
        public string Move { get; set; }

        public string Description()
        {
            var repo = new GameRepository();
            return repo.FindById(GameId).Description;
        }
    }
}