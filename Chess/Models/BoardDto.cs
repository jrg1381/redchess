using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Entity;
using System.Linq;
using RedChess.ChessCommon.Enumerations;
using RedChess.ChessCommon.Interfaces;

namespace Chess.Models
{
    public class BoardDto
    {
        private readonly IBoard m_board;
        private bool m_gameOver;
        private bool m_canClaimDraw;
        private DateTime m_creationDate;

        public BoardDto()
        {
            m_board = new BoardImpl();
        }

        public BoardDto(IBoard board, int owner, int opponent)
        {
            m_board = board;
            UserIdWhite = owner;
            UserIdBlack = opponent;
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
            using (var dbContext = new ChessContext())
            {
                int userId = dbContext.PlayerId(userName);

                if (userId == UserIdBlack)
                    return "b";
                if (userId == UserIdWhite)
                    return "w";
                return "";
            }
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
                    var whiteProfile = dbContext.UserProfiles.Find(UserIdWhite);
                    var blackProfile = dbContext.UserProfiles.Find(UserIdBlack);
                    return String.Format("{0} vs {1}", whiteProfile.UserName, blackProfile.UserName);
                }
            }
        }

        public void UpdateMessage()
        {
            // TODO: column indicating that a game is over
            if (Status == "Resigned")
                return;

            Status = String.Empty;	

            if (m_board.KingInCheck())
            {
                Status = "Check!";
                if (m_board.IsCheckmate(true))
                {
                    GameOver = true;
                    Status = "Checkmate!";
                }
            }
            else if (m_board.IsStalemate())
            {
                GameOver = true;
                Status = "Stalemate :-(";
            }
            else if (m_board.IsDraw())
            {
                GameOver = true;
                Status = "Insufficient material - draw";
            }	
        }

        public bool GameOver { get { return m_gameOver; } set { m_gameOver |= value; } }
        public string Fen { get { return m_board.ToFen(); } set { m_board.FromFen(value); } }

        [DatabaseGenerated(DatabaseGeneratedOption.Computed)]
        public bool MayClaimDraw { get { return m_canClaimDraw; } set { m_canClaimDraw = value; }}

        public bool Move(Location start, Location end) { return m_board.Move(start, end); }
        public void PromotePiece(string typeToPromoteTo) { m_board.PromotePiece(typeToPromoteTo); }

        [DatabaseGenerated(DatabaseGeneratedOption.Computed)]
        public DateTime CreationDate
        {
            get { return new DateTime(m_creationDate.Ticks, DateTimeKind.Utc); }
            set { m_creationDate = value; }
        }

        public void EndGameWithMessage(string message)
        {
            Status = message;
            GameOver = true;
        }
    }
}