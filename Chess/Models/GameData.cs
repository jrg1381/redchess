using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Data.SqlTypes;

namespace Chess.Models
{
    [Table("Boards")]
    public class GameData
    {
        private DateTime m_creationDate;
        private DateTime? m_completionDate;
        private bool m_gameOver;

        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int GameId { get; set; }

        public int UserIdWhite { get; set; }
        public int UserIdBlack { get; set; }
        public string Status { get; set; }
        public bool GameOver { get { return m_gameOver; } set { m_gameOver |= value; } }
        public string Fen { get; set; }
        public string LastMove { get; set; }

        [DatabaseGenerated(DatabaseGeneratedOption.Computed)]
        public bool MayClaimDraw { get; set; }

        [DatabaseGenerated(DatabaseGeneratedOption.Computed)]
        public DateTime CreationDate
        {
            get { return new DateTime(m_creationDate.Ticks, DateTimeKind.Utc); }
            set { m_creationDate = value; }
        }

        public DateTime CompletionDate
        {
            get { return m_completionDate.HasValue ? new DateTime(m_completionDate.Value.Ticks, DateTimeKind.Utc) : SqlDateTime.MinValue.Value; }
            set { m_completionDate = value; }
        }
    }
}