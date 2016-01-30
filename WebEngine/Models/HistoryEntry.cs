using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using RedChess.ChessCommon.Interfaces;
using RedChess.WebEngine.Repositories;

namespace RedChess.WebEngine.Models
{
    public class HistoryEntry : IHistoryEntry
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int HistoryId { get; set; }
        public int GameId { get; set; }
        public int MoveNumber { get; set; }
        public string Fen { get; set; }
        public string Move { get; set; }

        public HistoryEntry(GameDto gameDto)
        {
            Fen = gameDto.Fen;
            GameId = gameDto.GameId;
            Move = gameDto.LastMove;
            MoveNumber = gameDto.MoveNumber + 1;
        }

        public HistoryEntry()
        { }
    }
}