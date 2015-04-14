using System;
using System.Linq;
using System.Web.Mvc;
using Chess.Models;
using Chess.Repositories;
using RedChess.EngineFactory;

namespace Chess.Controllers
{
    public class HistoryController : Controller
    {
        readonly GameRepository m_gameRepository = new GameRepository();
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
            string opponent;

            var thisGame = m_gameRepository.FindById(game);

            if (thisGame == null)
            {
                return View("Error", new HandleErrorInfo(new ArgumentException("Source game not found in database"), "History", "PlayFromHere"));
            }

            if (thisGame.UserProfileBlack.UserName == m_identityProvider.CurrentUser)
            {
                opponent = thisGame.UserIdWhite.ToString();
                playAsBlack = true;
            }
            else
            {
                opponent = thisGame.UserIdBlack.ToString();
            }

            var historyEntry = m_historyRepository.FindByGameIdAndMoveNumber(game, moveNumber + 1);
            if (historyEntry == null)
            {
                return View("Error", new HandleErrorInfo(new ArgumentException("Source game not found in history"), "History", "PlayFromHere"));
            }

            var newBoard = new BoardImpl();
            newBoard.FromFen(historyEntry.Fen);

            var bc = new BoardController();
            return bc.Create(newBoard, opponent, false, String.Empty, playAsBlack);
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
