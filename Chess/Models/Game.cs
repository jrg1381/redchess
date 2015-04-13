using System;
using System.Linq;
using Chess.Controllers;
using RedChess.ChessCommon.Enumerations;
using RedChess.ChessCommon.Interfaces;

namespace Chess.Models
{
    public class Game : IDisposable
    {
        private readonly IBoard m_board;
        private GameData m_gameData;

        public Game()
        {
            m_board = new BoardImpl();
            m_gameData = new GameData();
        }

        public Game(GameData data)
        {
            m_board = new BoardImpl();
            m_gameData = data;
        }

        public Game(int id)
        {
            using (var context = new ChessContext())
            {
                m_gameData = context.Boards.Find(id);
                m_board = new BoardImpl();
                m_board.FromFen(m_gameData.Fen);
            }
        }

        public Game(IBoard board, int owner, int opponent)
        {
            m_board = board;
            m_gameData = new GameData
            {
                UserIdWhite = owner,
                UserIdBlack = opponent, 
                Status = String.Empty, 
                Fen = m_board.ToFen()
            };
        }

        public int GameId { get { return m_gameData.GameId; } }

        public string Turn
        {
            get { return m_board.CurrentTurn.ToString(); } 
        }

        public bool IsUsersTurn(int userId)
        {
            return (m_board.CurrentTurn == PieceColor.Black && userId == m_gameData.UserIdBlack) ||
                   (m_board.CurrentTurn == PieceColor.White && userId == m_gameData.UserIdWhite);
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

            if (userId == m_gameData.UserIdBlack)
                return "b";
            if (userId == m_gameData.UserIdWhite)
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

        public string Description
        {
            get
            {
                using (var dbContext = new ChessContext())
                {
                    var whiteProfile = UserUtilities.UserProfileFromId(dbContext, m_gameData.UserIdWhite);
                    var blackProfile = UserUtilities.UserProfileFromId(dbContext, m_gameData.UserIdBlack);
                    return String.Format("{0} vs {1}", whiteProfile.UserName, blackProfile.UserName);
                }
            }
        }

        public void UpdateMessage()
        {
            m_gameData.Status = String.Empty;

            if (m_board.KingInCheck())
            {
                m_gameData.Status = "Check!";
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

        public bool Move(Location start, Location end)
        {
            var success = m_board.Move(start, end);
            if (success)
            {
                using (var context = new ChessContext())
                {
                    m_gameData = context.Boards.Find(GameId);
                    m_gameData.LastMove = m_board.LastMove();
                    m_gameData.Fen = m_board.ToFen();
                    context.SaveChanges();
                }
            }
            return success;
        }

        public void PromotePiece(string typeToPromoteTo)
        {
            using (var context = new ChessContext())
            {
                m_gameData = context.Boards.Find(GameId);
                m_board.PromotePiece(typeToPromoteTo);
                m_gameData.LastMove = m_board.LastMove();
                m_gameData.Fen = m_board.ToFen();
                context.SaveChanges();
            }
        }

        public string Status { get { return m_gameData.Status; } }
        public int UserIdWhite { get { return m_gameData.UserIdWhite; } }
        public int UserIdBlack { get { return m_gameData.UserIdBlack; } }
        public bool MayClaimDraw { get { return m_gameData.MayClaimDraw; } }
        public bool GameOver { get { return m_gameData.GameOver; } }
        public GameData Data { get { return m_gameData;} }
        public DateTime CreationDate { get { return m_gameData.CreationDate; } }
        public DateTime CompletionDate { get { return m_gameData.CompletionDate; } }
        public string LastMove { get { return m_gameData.LastMove; }}
        public string Fen { get { return m_gameData.Fen; } }

        public void EndGameWithMessage(string message)
        {
            using (var context = new ChessContext())
            {
                m_gameData = context.Boards.Find(GameId);
                m_gameData.Status = message;
                m_gameData.CompletionDate = DateTime.UtcNow;
                m_gameData.GameOver = true;
                context.SaveChanges();
            }
        }

        public void Dispose()
        {
            m_board.Dispose();
        }
    }
}