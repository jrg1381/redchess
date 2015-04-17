using System;
using System.Collections.Generic;
using System.Linq;
using RedChess.ChessCommon.Enumerations;
using RedChess.ChessCommon.Interfaces;
using RedChess.WebEngine.Models;
using RedChess.EngineFactory;

namespace RedChess.WebEngine.Repositories
{
    public class GameManager : IGameManager
    {
        private readonly IGameRepository m_repository;
        private readonly IHistoryRepository m_historyRepository;
        private readonly IClockRepository m_clockRepository;
        private readonly IBoard m_board;

        public GameManager(IGameRepository gameRepository = null, IHistoryRepository historyRepository = null, IClockRepository clockRepository = null)
        {
            m_repository = gameRepository ?? new GameRepository();
            m_historyRepository = historyRepository ?? new HistoryRepository();
            m_clockRepository = clockRepository ?? new ClockRepository();
            m_board = BoardFactory.CreateInstance();
        }

        public void SaveClock(IClock clock)
        {
            m_clockRepository.SaveClock(clock);
        }

        public IClock Clock(int gameId)
        {
            return m_clockRepository.Clock(gameId);
        }

        public HistoryEntry FindByGameIdAndMoveNumber(int gameId, int moveNumber)
        {
            return m_historyRepository.FindByGameIdAndMoveNumber(gameId, moveNumber);
        }

        public IEnumerable<HistoryEntry> FindAllMoves(int gameId)
        {
            return m_historyRepository.FindAllMoves(gameId);
        }

        public void TimeGameOut(int gameId, string message, string userName)
        {
            var game = m_repository.FindById(gameId);

            // Because the user who timed out hasn't made a move to update the clock, the database will contains an elapsed time
            // which is too small. It will be equal to the time spent on all their _completed_ moves, and not the time they spent
            // thinking about the last one. This has the annoying effect that reloading the page will increase their time from 0.
            // To fix this, edit the loser's elapsed time in the database and set it equal to the time limit for the game.

            var clock = m_clockRepository.Clock(gameId);
            var timeLimit = clock.TimeLimitMs;

            if (userName == game.UserProfileWhite.UserName)
            {
                clock.TimeElapsedWhiteMs = timeLimit;
            }
            if (userName == game.UserProfileBlack.UserName)
            {
                clock.TimeElapsedBlackMs = timeLimit;
            }

            m_clockRepository.SaveClock(clock);
            EndGameWithMessage(gameId, message);
        }

        public bool ShouldLockUi(int gameId)
        {
            var clock = m_clockRepository.Clock(gameId);

            if (clock == null)
                return false;

            if (clock.PlayersReady != 3)
                return true;

            return false;
        }

        public string Turn(int gameId)
        {
            var gameDto = m_repository.FindById(gameId);
            m_board.FromFen(gameDto.Fen);
            return m_board.CurrentTurn.ToString();
        }

        public bool IsUsersTurn(int gameId, string userName)
        {
            var gameDto = m_repository.FindById(gameId);
            m_board.FromFen(gameDto.Fen);
            return (m_board.CurrentTurn == PieceColor.Black && userName == gameDto.UserProfileBlack.UserName) ||
                   (m_board.CurrentTurn == PieceColor.White && userName == gameDto.UserProfileWhite.UserName);
        }

        /// <summary>
        /// Return 'b' or 'w' or the empty string.
        /// </summary>
        /// <param name="gameId"></param>
        /// <param name="userName"></param>
        /// <returns></returns>
        public string CurrentPlayerColor(int gameId, string userName)
        {
            var gameDto = m_repository.FindById(gameId);

            if (gameDto.UserProfileBlack.UserName == userName)
                return "b";
            if (gameDto.UserProfileWhite.UserName == userName)
                return "w";

            return "";
        }

        public string Description(int gameId)
        {
            var gameDto = m_repository.FindById(gameId);
            return String.Format("{0} vs {1}", gameDto.UserProfileWhite.UserName, gameDto.UserProfileBlack.UserName);
        }

        public bool Move(int gameId, Location start, Location end)
        {
            var gameDto = m_repository.FindById(gameId);
            m_board.FromFen(gameDto.Fen);

            var success = m_board.Move(start, end);
            if (!success) return false;

            gameDto.LastMove = m_board.LastMove();
            gameDto.Fen = m_board.ToFen();

            m_repository.AddOrUpdate(gameDto);

            var moveNumber = m_historyRepository.LatestMoveInGame(gameId) + 1;
            m_historyRepository.Add(new HistoryEntry {Fen = gameDto.Fen, GameId = gameDto.GameId, Move = gameDto.LastMove, MoveNumber = moveNumber});

            var clock = m_clockRepository.Clock(gameId);

            if (clock != null)
            {
                if (Turn(gameId) == "Black")
                {
                    clock.LastActionBlack = DateTime.UtcNow;
                    clock.TimeElapsedWhiteMs += (int) (DateTime.UtcNow - clock.LastActionWhite).TotalMilliseconds;
                }
                if (Turn(gameId) == "White")
                {
                    clock.LastActionWhite = DateTime.UtcNow;
                    clock.TimeElapsedBlackMs += (int) (DateTime.UtcNow - clock.LastActionBlack).TotalMilliseconds;
                }

                m_clockRepository.SaveClock(clock);
            }

            return true;
        }

        public void PromotePiece(int gameId, string typeToPromoteTo)
        {
            var gameDto = m_repository.FindById(gameId);
            m_board.FromFen(gameDto.Fen);
            m_board.PromotePiece(typeToPromoteTo);
            gameDto.LastMove = m_board.LastMove();
            gameDto.Fen = m_board.ToFen();
            m_repository.AddOrUpdate(gameDto);
        }

        public void UpdateMessage(int gameId)
        {
            var gameDto = m_repository.FindById(gameId);
            m_board.FromFen(gameDto.Fen);

            if (m_board.KingInCheck())
            {
                gameDto.Status = "Check";
                if (m_board.IsCheckmate(true))
                {
                    EndGameWithMessage(gameDto, "Checkmate");
                    return;
                }
            }
            else if (m_board.IsStalemate())
            {
                EndGameWithMessage(gameDto, "Stalemate");
                return;
            }
            else if (m_board.IsDraw())
            {
                EndGameWithMessage(gameDto, "Insufficient material - draw");
                return;
            }
            else
            {
                gameDto.Status = "";
            }

            m_repository.AddOrUpdate(gameDto);
        }

        public void EndGameWithMessage(int gameId, string message)
        {
            var gameDto = m_repository.FindById(gameId);
            EndGameWithMessage(gameDto, message);
        }

        internal void EndGameWithMessage(GameDto gameDto, string message)
        {
            gameDto.Status = message;
            gameDto.CompletionDate = DateTime.UtcNow;
            gameDto.GameOver = true;
            m_repository.AddOrUpdate(gameDto);
        }

        public IGameBinding FetchGame(int gameId)
        {
            return new GameBinding(m_repository.FindById(gameId), this);
        }

        public IEnumerable<IGameBinding> FindAll()
        {
            return m_repository.FindAll().Select(x => new GameBinding(x, this));
        }

        public void Delete(int gameId)
        {
            m_repository.Delete(gameId);
        }

        public int Add(IBoard board, int opponentId, string currentUser, bool playAsBlack, int timeLimitMs)
        {
            var newGame = new GameDto {Fen = board.ToFen()};
            var currentUserId = (new UserProfileRepository()).UserId(currentUser);

            if (playAsBlack)
            {
                newGame.UserIdBlack = currentUserId;
                newGame.UserIdWhite = opponentId;
            }
            else
            {
                newGame.UserIdBlack = opponentId;
                newGame.UserIdWhite = currentUserId;
            }

            m_repository.AddOrUpdate(newGame);

            if (timeLimitMs != 0)
            {
                m_clockRepository.AddClock(newGame.GameId, timeLimitMs);
            }

            m_historyRepository.Add(new HistoryEntry { Fen = newGame.Fen, GameId = newGame.GameId, Move = ""});

            return newGame.GameId;
        }
    }
}