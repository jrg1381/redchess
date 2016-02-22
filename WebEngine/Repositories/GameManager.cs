using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;
using RedChess.ChessCommon;
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
        private readonly Lazy<IBoard> m_boardLazy;
        private readonly IGameRepository m_gameRepository;
        private readonly IHistoryRepository m_historyRepository;
        private readonly IClockRepository m_clockRepository;
        private readonly IQueueManager m_queueManager;
        private readonly IUserProfileRepository m_userRepository;
        private readonly IAnalysisRepository m_analysisRepository;
        private readonly IStatsRepository m_statsRepository;
        private readonly IDateTimeProvider m_dateTimeProvider;

        private IBoard m_board => m_boardLazy.Value;

        public GameManager() : this(null, null, null, null, null, null, null)
        {
        }

        internal GameManager(IGameRepository gameRepository = null, 
            IHistoryRepository historyRepository = null, 
            IClockRepository clockRepository = null, 
            IQueueManager queueManager = null,
            IUserProfileRepository userProfileRepository = null,
            IAnalysisRepository analysisRepository = null,
            IStatsRepository statsRepository = null,
            IDateTimeProvider dateTimeProvider = null)
        {
            m_gameRepository = gameRepository ?? new GameRepository();
            m_historyRepository = historyRepository ?? new HistoryRepository();
            m_clockRepository = clockRepository ?? new ClockRepository();
            m_queueManager = queueManager ?? QueueManagerFactory.CreateInstance();
            m_userRepository = userProfileRepository ?? new UserProfileRepository();
            m_analysisRepository = analysisRepository ?? new AnalysisRepository();
            m_statsRepository = statsRepository ?? new StatsRepository();
            m_dateTimeProvider = dateTimeProvider ?? new DefaultDateTimeProvider();

            // The repositories are fast to create, but this can be slow. Not all calls use the board.
            m_boardLazy = new Lazy<IBoard>(BoardFactory.CreateInstance);
        }

        public IEnumerable<IAnalysisBinding> AnalysisForGameMoves(int gameId)
        {
            return m_analysisRepository.AnalysisForGameMoves(gameId);
        }

        public IEnumerable<IStats> Stats()
        {
            return m_statsRepository.Stats().Select(x => new Stats()
            {
                Black = x.Black.ToLower(),
                White = x.White.ToLower(),
                Winner = x.Winner?.ToLower(),
                Count = x.Count
            });
        }

        public object FindWhere(string queryString)
        {
            return m_gameRepository.FindWhere(queryString);
        }

        public IEnumerable<UserProfile> AllUserProfiles()
        {
            return m_userRepository.FindAll();
        }

        public void AddAnalysis(int gameId, int moveNumber, IBoardAnalysis boardAnalysis)
        {
            var processor = new AnalysisSimplifier(m_historyRepository);
            var processedAnalysis = processor.ProcessBoardAnalysis(gameId, moveNumber, boardAnalysis);
            m_gameRepository.AddAnalysis(gameId, moveNumber, processedAnalysis);
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

            // Non-default start position (game cloned from another)
            if (numberOfMoves > 0 && entries[0].Fen != "rnbqkbnr/pppppppp/8/8/8/8/PPPPPPPP/RNBQKBNR w KQkq - 0")
            {
                pgnBuilder.AppendFormat("[FEN \"{0}\"]\r\n", entries[0].Fen);
            }

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

            m_gameRepository.AddOrUpdate(newGame);
            m_historyRepository.CloneGame(newGame.GameId, oldGameId, cloneUpToMove);
            m_analysisRepository.CloneGame(newGame.GameId, oldGameId, cloneUpToMove);

            return newGame.GameId;
        }

        public void TimeGameOut(int gameId, string message, string timedOutColor)
        {
            var game = m_gameRepository.FindById(gameId);

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

            if (clock.PlayersReady != PlayerReadyStatus.Both)
                return true;

            return false;
        }

        public bool IsUsersTurn(IGameBinding game, string userName)
        {
            var whiteToPlay = game.Fen.Contains("w");

            return (!whiteToPlay && userName == game.UserProfileBlack.UserName) ||
                   (whiteToPlay && userName == game.UserProfileWhite.UserName);
        }

        private string LongAlgebraicMove(ChessMove move)
        {
            return $"{move.Start}{move.End}{move.Promotion ?? ""}".ToLower();
        }

        /// <summary>
        /// Move a piece from start to end with optional promotion. Returns true if the move was successful (valid)
        /// </summary>
        /// <param name="gameId"></param>
        /// <param name="start"></param>
        /// <param name="end"></param>
        /// <param name="promote">A string beginning with one of "Q", "R", "B", "N"</param>
        /// <param name="moveReceivedAt"></param>
        /// <param name="newBoard">Board after the move is made</param>
        /// <returns></returns>
        public bool Move(int gameId, Location start, Location end, string promote, DateTime moveReceivedAt, out GameDto newBoard)
        {
            ChessMove move;
            var gameDto = m_gameRepository.FindById(gameId);
            newBoard = gameDto;

            m_board.FromFen(gameDto.Fen);

            var success = m_board.Move(start, end);
            if (!success) return false;

            if (!String.IsNullOrEmpty(promote)) // UI passes this from form
            {
                m_board.PromotePiece(promote);
                move = new ChessMove(start, end, promote[0].ToString());
            }
            else
            {
                move = new ChessMove(start, end);
            }

            // We post the move with the old fen and movenumber
            PostGameToQueueForBestMove(gameId, gameDto.MoveNumber, gameDto.Fen, LongAlgebraicMove(move));

            var fen = m_board.ToFen();
            var status = m_board.StatusForBoard();
            newBoard = m_gameRepository.RecordMove(gameId, fen, m_board.LastMove(), moveReceivedAt, status);
            if (status.GameOver())
            {
                PostGameEndedMessage(gameId);
            }

            return true;
        }

        public void EndGameWithMessage(int gameId, string message, int? userIdWinner = null)
        {
            var gameDto = m_gameRepository.FindById(gameId);
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
            if (userIdWinner != null)
                gameDto.UserIdWinner = userIdWinner;
            m_gameRepository.AddOrUpdate(gameDto);
        }

        public IGameBinding FetchGame(int gameId)
        {
            var dto = m_gameRepository.FindById(gameId);
            if (dto == null)
                return null;
            return new GameBinding(dto, this);
        }

        public void Delete(int gameId)
        {
            m_gameRepository.Delete(gameId);
            PostGameEndedMessage(gameId);
        }

        public void UpdateEloTable()
        {
            m_statsRepository.UpdateEloTable();
        }

        public int Add(IBoard board, string user)
        {
            var currentUserId = m_userRepository.UserId(user);

            var newGame = new GameDto
            {
                Fen = board.ToFen(), UserIdBlack = currentUserId, UserIdWhite = currentUserId
            };

            m_gameRepository.AddOrUpdate(newGame);
            m_historyRepository.Add(new HistoryEntry {Fen = newGame.Fen, GameId = newGame.GameId, Move = ""});

            return newGame.GameId;
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

            // Update to get the id for the newly created game
            m_gameRepository.AddOrUpdate(newGame);

            if (timeLimitMs != 0)
            {
                var clockId = m_clockRepository.AddClock(newGame.GameId, timeLimitMs);
                newGame.ClockId = clockId;
            }

            // Update again to force save of the clockId in the GameDto
            m_gameRepository.AddOrUpdate(newGame);

            m_historyRepository.Add(new HistoryEntry {Fen = newGame.Fen, GameId = newGame.GameId, Move = ""});

            return newGame.GameId;
        }

        public PlayerReadyStatus PlayerReady(int gameId, string playerColor)
        {
            return m_clockRepository.PlayerReady(gameId, playerColor);
        }

        public string GetEmailHashForUsername(string username)
        {
            return m_userRepository.GetEmailHashForUsername(username);
        }

        public void SetEmailHashForUserId(int userId, string emailHash)
        {
            m_userRepository.SetEmailHashForUserId(userId, emailHash);
        }

        public IEnumerable<IEloHistory> EloTable()
        {
            return m_statsRepository.Elo();
        }

        public DateTime LastEloUpdate()
        {
            return m_statsRepository.LastUpdate();
        }
    }
}
