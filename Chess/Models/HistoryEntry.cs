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

        public int LastMove()
        {
            using(var db = new ChessContext())
            {
                return db.HistoryEntries.Where(entry => entry.GameId == GameId).Max(entry => entry.MoveNumber);
            }
        }

        public string Description()
        {
            using (var db = new ChessContext())
            {
                return db.Boards.First(g => g.GameId == GameId).Description;
            }
        }

        public bool IsParticipant(string username)
        {
            using (var db = new ChessContext())
            {
                int userId = UserUtilities.UserProfileFromName(db, username).UserId;
                if (userId == -1)
                    return false;

                return db.Boards.Any(g => g.GameId == GameId && (g.UserIdBlack == userId || g.UserIdWhite == userId));
            }
        }
    }
}