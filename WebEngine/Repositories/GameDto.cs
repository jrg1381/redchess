using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Data.SqlTypes;
using RedChess.ChessCommon.Enumerations;
using RedChess.WebEngine.Models;

namespace RedChess.WebEngine.Repositories
{
    [Table("Boards")]
    public class GameDto
    {
        private DateTime m_CreationDate;
        private DateTime? m_CompletionDate;
        private bool m_GameOver;

        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int GameId { get; set; }

        public int? ClockId { get; set; }
        public int UserIdWhite { get; set; }
        public int UserIdBlack { get; set; }
        public int? UserIdWinner { get; set; }
        public string Status { get; set; }
        public bool GameOver { get { return m_GameOver; } set { m_GameOver |= value; } }
        public string Fen { get; set; }
        public string LastMove { get; set; }
        public Location LastMoveStart { get; set; }
        public Location LastMoveEnd { get; set; }
        
        [DatabaseGenerated(DatabaseGeneratedOption.Computed)] // Not really computed, actually set by a trigger
        public int MoveNumber { get; set; }

        [DatabaseGenerated(DatabaseGeneratedOption.Computed)]
        public bool MayClaimDraw { get; set; }

        [DatabaseGenerated(DatabaseGeneratedOption.Computed)]
        public DateTime CreationDate
        {
            get { return new DateTime(m_CreationDate.Ticks, DateTimeKind.Utc); }
            set { m_CreationDate = value; }
        }

        public DateTime CompletionDate
        {
            get { return m_CompletionDate.HasValue ? new DateTime(m_CompletionDate.Value.Ticks, DateTimeKind.Utc) : SqlDateTime.MinValue.Value; }
            set { m_CompletionDate = value; }
        }

        [ForeignKey("UserIdWhite")]
        public virtual UserProfile UserProfileWhite { get; set; }
        [ForeignKey("UserIdBlack")]
        public virtual UserProfile UserProfileBlack { get; set; }
        [ForeignKey("UserIdWinner")]
        public virtual UserProfile UserProfileWinner { get; set; }
        [ForeignKey("ClockId")]
        public virtual Clock Clock { get; set; }
    }
}