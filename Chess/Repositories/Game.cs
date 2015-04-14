using System;
using System.Linq;
using Chess.Models;
using RedChess.ChessCommon.Enumerations;
using RedChess.ChessCommon.Interfaces;
using RedChess.EngineFactory;

namespace Chess.Repositories
{
    public class Game : IGame
    {
        private GameDto m_gameDto;
        private readonly IBoard m_board;

        public Game(IBoard board = null)
        {
            m_board = board ?? BoardFactory.CreateInstance();
        }

        public Game(GameDto game)
        {
            m_gameDto = game;
            m_board = BoardFactory.CreateInstance();
            m_board.FromFen(m_gameDto.Fen);
        }

        public IClock Clock
        {
            get
            {
                return (new ClockRepository()).Clock(m_gameDto.GameId);
            }
        }

        public bool ShouldLockUi
        {
            get
            {
                var clock = (new ClockRepository()).Clock(m_gameDto.GameId);

                if (clock == null)
                    return false;

                if (clock.PlayersReady != 3 || clock.TimeElapsedBlackMs <= clock.TimeLimitMs || clock.TimeElapsedWhiteMs <= clock.TimeLimitMs)
                    return true;

                return false;
            }
        }

        public string Turn
        {
            get { return m_board.CurrentTurn.ToString(); }
        }

        public bool IsUsersTurn(string userName)
        {
            using (var context = new ChessContext())
            {
                m_gameDto = context.Boards.Find(m_gameDto.GameId);
                m_board.FromFen(m_gameDto.Fen);
                return (m_board.CurrentTurn == PieceColor.Black && userName == m_gameDto.UserProfileBlack.UserName) ||
                       (m_board.CurrentTurn == PieceColor.White && userName == m_gameDto.UserProfileWhite.UserName);
            }
        }

        /// <summary>
        /// Return 'b' or 'w' or the empty string.
        /// </summary>
        /// <param name="userName"></param>
        /// <returns></returns>
        public string CurrentPlayerColor(string userName)
        {
            using (var context = new ChessContext())
            {
                m_gameDto = context.Boards.Find(m_gameDto.GameId);
                if (m_gameDto.UserProfileBlack.UserName == userName)
                    return "b";
                if (m_gameDto.UserProfileWhite.UserName == userName)
                    return "w";

                return "";
            }
        }

        public string Description
        {
            get
            {
                using (var context = new ChessContext())
                {
                    m_gameDto = context.Boards.Find(m_gameDto.GameId);
                    return String.Format("{0} vs {1}", m_gameDto.UserProfileWhite.UserName,
                        m_gameDto.UserProfileBlack.UserName);
                }
            }
        }

        public bool Move(Location start, Location end)
        {
            using (var context = new ChessContext())
            {
                m_gameDto = context.Boards.Find(m_gameDto.GameId);
                m_board.FromFen(m_gameDto.Fen);

                var success = m_board.Move(start, end);
                if (!success) return false;

                m_gameDto.LastMove = m_board.LastMove();
                m_gameDto.Fen = m_board.ToFen();
                context.SaveChanges();

                int nextMoveNumber = context.HistoryEntries.Where(x => x.GameId == m_gameDto.GameId).Max(x => x.MoveNumber) + 1;
                context.HistoryEntries.Add(new HistoryEntry() { Fen = m_gameDto.Fen, GameId = m_gameDto.GameId, MoveNumber = nextMoveNumber, Move = m_gameDto.LastMove });

                var clock = context.Clocks.FirstOrDefault(c => c.GameId == Id);

                if (clock != null)
                {
                    var f = new Game(m_gameDto);

                    if (f.Turn == "Black")
                    {
                        clock.LastActionBlack = DateTime.UtcNow;
                        clock.TimeElapsedWhiteMs += (int)(DateTime.UtcNow - clock.LastActionWhite).TotalMilliseconds;
                    }
                    if (f.Turn == "White")
                    {
                        clock.LastActionWhite = DateTime.UtcNow;
                        clock.TimeElapsedBlackMs += (int)(DateTime.UtcNow - clock.LastActionBlack).TotalMilliseconds;
                    }
                }

                context.SaveChanges();
            }

            return true;
        }

        public void PromotePiece(string typeToPromoteTo)
        {
            using (var context = new ChessContext())
            {
                m_gameDto = context.Boards.Find(m_gameDto.GameId);
                m_board.PromotePiece(typeToPromoteTo);
                m_gameDto.LastMove = m_board.LastMove();
                m_gameDto.Fen = m_board.ToFen();
                context.SaveChanges();
            }
        }

        public void UpdateMessage(int gameId)
        {
            using (var context = new ChessContext())
            {
                m_gameDto = context.Boards.Find(gameId);
                m_board.FromFen(m_gameDto.Fen);

                if (m_board.KingInCheck())
                {
                    m_gameDto.Status = "Check!";
                    if (m_board.IsCheckmate(true))
                    {
                        EndGameWithMessage(gameId, "Checkmate!");
                    }
                }
                else if (m_board.IsStalemate())
                {
                    EndGameWithMessage(gameId, "Stalemate :-(");
                }
                else if (m_board.IsDraw())
                {
                    EndGameWithMessage(gameId, "Insufficient material - draw");
                }
                else
                {
                    m_gameDto.Status = "";
                    context.SaveChanges();
                }
            }
        }

        public void EndGameWithMessage(int gameId, string message)
        {
            using (var context = new ChessContext())
            {
                m_gameDto = context.Boards.Find(gameId);
                m_gameDto.Status = message;
                m_gameDto.CompletionDate = DateTime.UtcNow;
                m_gameDto.GameOver = true;
                context.SaveChanges();
            }
        }

        #region delegated to data transfer object

        public int Id
        {
            get { return m_gameDto.GameId; }
        }

        public string Status
        {
            get { return m_gameDto.Status; }
        }

        public int UserIdWhite
        {
            get { return m_gameDto.UserIdWhite; }
        }

        public int UserIdBlack
        {
            get { return m_gameDto.UserIdBlack; }
        }

        public bool MayClaimDraw
        {
            get { return m_gameDto.MayClaimDraw; }
        }

        public bool GameOver
        {
            get { return m_gameDto.GameOver; }
        }

        public DateTime CreationDate
        {
            get { return m_gameDto.CreationDate; }
        }

        public DateTime CompletionDate
        {
            get { return m_gameDto.CompletionDate; }
        }

        public string LastMove
        {
            get { return m_gameDto.LastMove; }
        }

        public string Fen
        {
            get { return m_gameDto.Fen; }
        }

        public UserProfile UserProfileWhite
        {
            get { return m_gameDto.UserProfileWhite; }
        }

        public UserProfile UserProfileBlack
        {
            get { return m_gameDto.UserProfileBlack; }
        }

        #endregion
    }
}