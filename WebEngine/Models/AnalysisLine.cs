using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace RedChess.WebEngine.Models
{
    [Table("AnalysisLines")]
    public class AnalysisLine
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int AnalysisLineId { get; set; }

        public int GameId { get; set; }
        public int AnalysisEntryId { get; set; }

        [ForeignKey("AnalysisEntryId")]
        public virtual AnalysisEntry ParentAnalysisEntry { get; set; }

    }
}