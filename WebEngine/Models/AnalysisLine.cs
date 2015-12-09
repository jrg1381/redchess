using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Newtonsoft.Json;
using RedChess.ChessCommon.Interfaces;

namespace RedChess.WebEngine.Models
{
    [Table("AnalysisLines")]
    public class AnalysisLine : IHistoryEntry
    {
        [Key]
        [JsonIgnore]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int AnalysisLineId { get; set; }

        [JsonIgnore]
        public int GameId { get; set; }
        [JsonIgnore]
        public int AnalysisEntryId { get; set; }

        public string Move { get; set; }
        public string Fen { get; set; }
        public int MoveNumber { get; set; }

        [ForeignKey("AnalysisEntryId")]
        [JsonIgnore]
        public virtual AnalysisEntry ParentAnalysisEntry { get; set; }

    }
}