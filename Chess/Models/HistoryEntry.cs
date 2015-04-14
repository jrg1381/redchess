using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using Chess.Controllers;

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

        public int LastMoveNumber()
        {
            using(var db = new ChessContext())
            {
                return db.HistoryEntries.Where(entry => entry.GameId == GameId).Max(entry => entry.MoveNumber);
            }
        }

        public string Description()
        {
            var repo = new GameRepository();
            return repo.FindById(GameId).Description;
        }

        public bool IsParticipant(string username)
        {
            using (var db = new ChessContext())
            {
                return db.Database.SqlQuery<bool>("SELECT dbo.IsParticipant(@p0,@p1)", GameId, username).FirstOrDefault();
            }
        }
    }
}