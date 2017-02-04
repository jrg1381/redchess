using System;
using System.Linq;
using System.Web.Mvc;
using RedChess.WebEngine.Repositories;
using RedChess.EngineFactory;
using RedChess.WebEngine.Repositories.Interfaces;

namespace Chess.Controllers
{
    public class HistoryController : Controller
    {
        readonly IGameManager m_GameManager;
        private readonly ICurrentUser m_IdentityProvider;

        public HistoryController(): this(null, null)
        { }

        public HistoryController(IGameManager gameManager = null, ICurrentUser identityProvider = null)
        {
            m_GameManager = gameManager ?? new GameManager();
            m_IdentityProvider = identityProvider ?? new CurrentUserProvider();
        }

        public ActionResult Index()
        {
			return HttpNotFound();
        }

        public ActionResult PlayFromHere(string move, string gameId)
        {
            bool playAsBlack = false;
            int moveNumber = Int32.Parse(move);
            int game = Int32.Parse(gameId);
            int opponent;

            var thisGame = m_GameManager.FetchGame(game);

            if (thisGame == null)
            {
                return View("Error", new HandleErrorInfo(new ArgumentException("Source game not found in database"), "History", "PlayFromHere"));
            }

            if (thisGame.UserProfileBlack.UserId == m_IdentityProvider.CurrentUserId)
            {
                opponent = thisGame.UserProfileWhite.UserId;
                playAsBlack = true;
            }
            else
            {
                opponent = thisGame.UserProfileBlack.UserId;
            }

            var historyEntry = m_GameManager.FindByGameIdAndMoveNumber(game, moveNumber);
            if (historyEntry == null)
            {
                return View("Error", new HandleErrorInfo(new ArgumentException("Source game not found in history"), "History", "PlayFromHere"));
            }

            var newBoard = BoardFactory.CreateInstance();
            newBoard.FromFen(historyEntry.Fen);

            int gameIdOfNewGame = m_GameManager.CloneBoard(newBoard, opponent, m_IdentityProvider.CurrentUser, playAsBlack, game, historyEntry.MoveNumber);

            return RedirectToAction("Details", "Board", new { id = gameIdOfNewGame});
        }

        public ActionResult ShowMove(int id)
		{
            var entries = m_GameManager.FindAllMoves(id);

            if (!entries.Any())
            {
                return HttpNotFound("No such game or move");
            }

            return View("History", id);
		}

        [HttpGet]
        public ContentResult Pgn(int id)
        {
            var content = new ContentResult
            {
                ContentType = @"text\plain",
                Content = m_GameManager.PgnText(id)
            };

            return content;
        }
    }
}
