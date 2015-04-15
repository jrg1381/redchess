using System;
using System.Linq;
using System.Web.Mvc;
using Chess.Models;
using RedChess.WebEngine.Repositories;
using RedChess.EngineFactory;

namespace Chess.Controllers
{
    public class HistoryController : Controller
    {
        readonly IGameManager m_gameManager = new GameManager();
        readonly HistoryRepository m_historyRepository = new HistoryRepository();
        private readonly ICurrentUser m_identityProvider = new CurrentUserImpl();

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

            var thisGame = m_gameManager.FetchGame(game);

            if (thisGame == null)
            {
                return View("Error", new HandleErrorInfo(new ArgumentException("Source game not found in database"), "History", "PlayFromHere"));
            }

            if (thisGame.UserProfileBlack.UserName == m_identityProvider.CurrentUser)
            {
                opponent = thisGame.UserProfileWhite.UserId;
                playAsBlack = true;
            }
            else
            {
                opponent = thisGame.UserProfileBlack.UserId;
            }

            var historyEntry = m_historyRepository.FindByGameIdAndMoveNumber(game, moveNumber);
            if (historyEntry == null)
            {
                return View("Error", new HandleErrorInfo(new ArgumentException("Source game not found in history"), "History", "PlayFromHere"));
            }

            var newBoard = BoardFactory.CreateInstance();
            newBoard.FromFen(historyEntry.Fen);

            int gameIdOfNewGame = m_gameManager.Add(newBoard, opponent, m_identityProvider.CurrentUser, playAsBlack, 0);
            return RedirectToAction("Details", "Board", new { id = gameIdOfNewGame});
        }

        public ActionResult ShowMove(string gameId, string moveNumber)
		{
			int game = Int32.Parse(gameId);
			int move = Int32.Parse(moveNumber);

            var entries = m_historyRepository.FindAllMoves(game);

            if (!entries.Any())
            {
                return HttpNotFound("No such game or move");
            }

            ViewBag.MoveNumber = move;
            return View("History", entries);
		}
    }
}
