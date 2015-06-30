using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using RedChess.WebEngine.Repositories;

namespace RedChess.WebEngine.Models
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
            var manager = new GameManager();
            return manager.FetchGame(GameId).Description;
        }

        public static bool IsParticipant(string userName, int gameId)
        {
            return (new HistoryRepository()).IsParticipant(userName, gameId);
        }
    }
}