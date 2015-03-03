using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Entity;
using System.Data.SqlTypes;
using System.Linq;
using Chess.Controllers;
using RedChess.ChessCommon.Enumerations;
using RedChess.ChessCommon.Interfaces;

namespace Chess.Models
{
    [Table("Boards")]
    public class Game
    {
        private readonly IBoard m_board;
        private bool m_gameOver;
        private bool m_canClaimDraw;
        private DateTime m_creationDate;
        private DateTime? m_completionDate;

        public Game()
        {
            m_board = new BoardImpl();
        }

        public Game(IBoard board, int owner, int opponent)
        {
            m_board = board;
            UserIdWhite = owner;
            UserIdBlack = opponent;
            Status = String.Empty;
        }

        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int GameId { get; set; }

        public int UserIdWhite { get; set; }
        public int UserIdBlack { get; set; }
        public string Status { get; set; }

        public string Turn
        {
            get { return m_board.CurrentTurn.ToString(); } 
        }

        public bool IsUsersTurn(int userId)
        {
            return (m_board.CurrentTurn == PieceColor.Black && userId == UserIdBlack) ||
                   (m_board.CurrentTurn == PieceColor.White && userId == UserIdWhite);
        }

        /// <summary>
        /// Return 'b' or 'w' or the empty string.
        /// </summary>
        /// <param name="userName"></param>
        /// <returns></returns>
        public string CurrentPlayerColor(string userName)
        {
            var profile = UserUtilities.UserProfileFromName(userName);
            if (profile == null)
                return "";

            int userId = profile.UserId;

            if (userId == UserIdBlack)
                return "b";
            if (userId == UserIdWhite)
                return "w";
            return "";
        }

        public Clock Clock()
        {
            using (var dbContext = new ChessContext())
            {
                return dbContext.Clocks.FirstOrDefault(clock => clock.GameId == GameId);
            }
        }

        [NotMapped]
        public string Description
        {
            get
            {
                using (var dbContext = new ChessContext())
                {
                    var whiteProfile = UserUtilities.UserProfileFromId(dbContext, UserIdWhite);
                    var blackProfile = UserUtilities.UserProfileFromId(dbContext, UserIdBlack);
                    return String.Format("{0} vs {1}", whiteProfile.UserName, blackProfile.UserName);
                }
            }
        }

        public void UpdateMessage()
        {
            Status = String.Empty;

            if (m_board.KingInCheck())
            {
                Status = "Check!";
                if (m_board.IsCheckmate(true))
                {
                    EndGameWithMessage("Checkmate!");
                }
            }
            else if (m_board.IsStalemate())
            {
                EndGameWithMessage("Stalemate :-(");
            }
            else if (m_board.IsDraw())
            {
                EndGameWithMessage("Insufficient material - draw");
            }
        }

        public bool GameOver { get { return m_gameOver; } set { m_gameOver |= value; } }
        public string Fen { get { return m_board.ToFen(); } set { m_board.FromFen(value); } }

        [DatabaseGenerated(DatabaseGeneratedOption.Computed)]
        public bool MayClaimDraw { get { return m_canClaimDraw; } set { m_canClaimDraw = value; }}

        public bool Move(Location start, Location end)
        {
            var success = m_board.Move(start, end);
            if (success)
                LastMove = m_board.LastMove();
            return success;
        }

        public void PromotePiece(string typeToPromoteTo)
        {
            m_board.PromotePiece(typeToPromoteTo);
            LastMove = m_board.LastMove();
        }

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

        public string LastMove { get; set; }

        public void EndGameWithMessage(string message)
        {
            Status = message;
            CompletionDate = DateTime.UtcNow;
            GameOver = true;
        }
    }
}