using System.Collections;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using RedChess.ChessCommon.Enumerations;

namespace RedChess.WebEngine.Models
{
    [Table("Analysis")]
    public class AnalysisEntry
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int AnalysisEntryId { get; set; }

        public int GameId { get; set; }
        public int MoveNumber { get; set; }
        public string Analysis { get; set; }
        public int Evaluation { get; set; }
        public EvaluationType EvaluationType { get; set; }

        [ForeignKey("AnalysisLineId")]
        public virtual IEnumerable<AnalysisLine> AnalysisLines { get; set; }
    }

    [Table("AnalysisLines")]
    public class AnalysisLine
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int AnalysisLineId { get; set; }
        public int GameId { get; set; }

    }
}