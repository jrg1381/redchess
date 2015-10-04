using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Emit;
using System.Text;
using System.Threading.Tasks;
using RedChess.ChessCommon.Enumerations;
using RedChess.ChessCommon.Interfaces;
using RedChess.MessageQueue;
using RedChess.WebEngine.Models;
using RedChess.EngineFactory;
using RedChess.WebEngine.Repositories.Interfaces;

namespace RedChess.WebEngine.Repositories
{
    public class GameManager : IGameManager
    {
        private readonly IGameRepository m_repository;
        private readonly IHistoryRepository m_historyRepository;
        private readonly IClockRepository m_clockRepository;
        private readonly IBoard m_board;
        private readonly IQueueManager m_queueManager;
        private readonly IUserProfileRepository m_userRepository;
        private readonly IAnalysisRepository m_analysisRepository;

        public GameManager() : this(null, null, null, null, null, null)
        {
        }

        internal GameManager(IGameRepository gameRepository = null, 
            IHistoryRepository historyRepository = null, 
            IClockRepository clockRepository = null, 
            IQueueManager queueManager = null,
            IUserProfileRepository userProfileRepository = null,
            IAnalysisRepository analysisRepository = null)
        {
            m_repository = gameRepository ?? new GameRepository();
            m_historyRepository = historyRepository ?? new HistoryRepository();
            m_clockRepository = clockRepository ?? new ClockRepository();
            m_queueManager = queueManager ?? QueueManagerFactory.CreateInstance();
            m_userRepository = userProfileRepository ?? new UserProfileRepository();
            m_analysisRepository = analysisRepository ?? new AnalysisRepository();
            m_board = BoardFactory.CreateInstance();
        }

        public IEnumerable<IAnalysisBinding> AnalysisForGameMoves(int gameId, int minMoveNumber, int maxMoveNumber)
        {
            return m_analysisRepository.AnalysisForGameMoves(gameId, minMoveNumber, maxMoveNumber);
        }

        public IEnumerable<UserProfile> AllUserProfiles()
        {
            return m_userRepository.FindAll();
        }

        public void AddAnalysis(int gameId, int moveNumber, string analysisText, int boardEvaluation)
        {
            m_repository.AddAnalysis(gameId, moveNumber, analysisText, boardEvaluation);
        }

        public string PgnText(int id)
        {
            var entries = FindAllMoves(id).ToList();

            if (!entries.Any())
            {
                return "NOT FOUND";
            }

            var gameDetails = FetchGame(id);
            return GeneratePgn(entries, gameDetails);
        }

        internal string GeneratePgn(IList<HistoryEntry> entries, IGameBinding gameDetails)
        {
            var numberOfMoves = entries.Count;
            var result = "*";

            if (gameDetails.GameOver)
            {
                if (gameDetails.UserProfileWinner == null)
                {
                    result = "1/2-1/2";
                }
                else
                {
                    if (gameDetails.UserProfileWhite.UserId == gameDetails.UserProfileWinner.UserId)
                    {
                        result = "1-0";
                    }

                    if (gameDetails.UserProfileBlack.UserId == gameDetails.UserProfileWinner.UserId)
                    {
                        result = "0-1";
                    }
                }
            }

            var pgnBuilder = new StringBuilder();

            pgnBuilder.AppendLine("[Event \"Casual Game\"]")
            .AppendLine("[Site \"?\"]")
            .AppendLine("[Round \"?\"]")
            .AppendFormat("[Date \"{0:yyyy.MM.dd}\"]\r\n", gameDetails.CreationDate)
            .AppendFormat("[White \"{0}\"]\r\n", gameDetails.UserProfileWhite.UserName)
            .AppendFormat("[Black \"{0}\"]\r\n", gameDetails.UserProfileBlack.UserName)
            .AppendFormat("[Result \"{0}\"]\r\n", result);

            if (gameDetails.Clock != null)
            {
                pgnBuilder.AppendFormat("[TimeControl \"{0}\"]\r\n", gameDetails.Clock.TimeLimitMs / 1000);
            }

            pgnBuilder.AppendLine();

            var moveNumber = 1;
            for (var i = 1; i < numberOfMoves; i += 2)
            {
                var nextMoveIndex = i + 1;
                pgnBuilder.AppendFormat("{0}. {1}", moveNumber++, entries[i].Move);
                if (nextMoveIndex < numberOfMoves)
                {
                    pgnBuilder.AppendFormat(" {0} ", entries[nextMoveIndex].Move);
                }
            }

            pgnBuilder.AppendFormat(" {0}", result);
            return pgnBuilder.ToString();
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

        public int CloneBoard(IBoard newBoard, int opponentId, string currentUser, bool playAsBlack, int oldGameId, int cloneUpToMove)
        {
            var newGame = new GameDto { Fen = newBoard.ToFen() };
            var currentUserId = m_userRepository.UserId(currentUser);

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
            m_historyRepository.CloneGame(newGame.GameId, oldGameId, cloneUpToMove);
            m_analysisRepository.CloneGame(newGame.GameId, oldGameId, cloneUpToMove);

            return newGame.GameId;
        }

        public void TimeGameOut(int gameId, string message, string timedOutColor)
        {
            var game = m_repository.FindById(gameId);

            // Because the user who timed out hasn't made a move to update the clock, the database will contains an elapsed time
            // which is too small. It will be equal to the time spent on all their _completed_ moves, and not the time they spent
            // thinking about the last one. This has the annoying effect that reloading the page will increase their time from 0.
            // To fix this, edit the loser's elapsed time in the database and set it equal to the time limit for the game.

            var clock = m_clockRepository.Clock(gameId);
            var timeLimit = clock.TimeLimitMs;

            if (timedOutColor == "w")
            {
                clock.TimeElapsedWhiteMs = timeLimit;
                game.UserIdWinner = game.UserIdBlack;
            }
            if (timedOutColor == "b")
            {
                clock.TimeElapsedBlackMs = timeLimit;
                game.UserIdWinner = game.UserIdWhite;
            }

            m_clockRepository.SaveClock(clock);
            EndGameWithMessage(game, message);
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

        private string LongAlgebraicMove(Location start, Location end, string promote)
        {
            return String.Format("{0}{1}{2}", start, end, promote ?? "").ToLower();
        }

        public bool Move(int gameId, Location start, Location end, string promote = null)
        {
            var gameDto = m_repository.FindById(gameId);
            m_board.FromFen(gameDto.Fen);

            var success = m_board.Move(start, end);
            if (!success) return false;

            if (promote != "") // UI passes this from form
            {
                m_board.PromotePiece(promote);
            }

            PostGameToQueueForBestMove(gameId, gameDto.MoveNumber, gameDto.Fen, LongAlgebraicMove(start, end, promote));

            gameDto.LastMove = m_board.LastMove();
            gameDto.Fen = m_board.ToFen();

            m_repository.AddOrUpdate(gameDto);

            m_historyRepository.Add(new HistoryEntry { Fen = gameDto.Fen, GameId = gameDto.GameId, Move = gameDto.LastMove, MoveNumber = gameDto.MoveNumber + 1});

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

        public void UpdateMessage(int gameId)
        {
            var gameDto = m_repository.FindById(gameId);
            m_board.FromFen(gameDto.Fen);

            if (m_board.KingInCheck())
            {
                gameDto.Status = "Check";
                if (m_board.IsCheckmate(true))
                {
                    var winner = m_board.CurrentTurn == PieceColor.White ? gameDto.UserIdBlack : gameDto.UserIdWhite;
                    EndGameWithMessage(gameDto, "Checkmate", winner);
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

        public void EndGameWithMessage(int gameId, string message, int? userIdWinner = null)
        {
            var gameDto = m_repository.FindById(gameId);
            EndGameWithMessage(gameDto, message, userIdWinner);
            PostGameEndedMessage(gameId);
        }

        private void PostGameEndedMessage(int gameId)
        {
            Task.Run(() => m_queueManager.PostGameEndedMessage(gameId));
        }

        private void PostGameToQueueForBestMove(int gameId, int moveId, string fen, string move)
        {
            Task.Run(() => m_queueManager.PostRequestBestMoveMessage(gameId, moveId, fen, move));
        }

        internal void EndGameWithMessage(GameDto gameDto, string message, int? userIdWinner = null)
        {
            gameDto.Status = message;
            gameDto.CompletionDate = DateTime.UtcNow;
            gameDto.GameOver = true;
            if(userIdWinner != null)
                gameDto.UserIdWinner = userIdWinner;
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
            PostGameEndedMessage(gameId);
        }

        public int Add(IBoard board, int opponentId, string currentUser, bool playAsBlack, int timeLimitMs)
        {
            var newGame = new GameDto {Fen = board.ToFen()};
            var currentUserId = m_userRepository.UserId(currentUser);

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

        public bool IsParticipant(string name, int gameId)
        {
            if (String.IsNullOrEmpty(name))
                return false;
            var game = m_repository.FindById(gameId);
            return game.UserProfileBlack.UserName == name || game.UserProfileWhite.UserName == name;
        }

        public IEnumerable<IGameBinding> WithPlayer(string userName)
        {
            return m_repository.FindWithPlayer(userName).Select(x => new GameBinding(x, this));
        }
    }
}