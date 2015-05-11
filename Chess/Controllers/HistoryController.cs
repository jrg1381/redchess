using System;
using System.Linq;
using System.Text;
using System.Web.Mvc;
using Chess.Models;
using RedChess.WebEngine.Repositories;
using RedChess.EngineFactory;

namespace Chess.Controllers
{
    public class HistoryController : Controller
    {
        readonly IGameManager m_gameManager = new GameManager();
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

            var historyEntry = m_gameManager.FindByGameIdAndMoveNumber(game, moveNumber);
            if (historyEntry == null)
            {
                return View("Error", new HandleErrorInfo(new ArgumentException("Source game not found in history"), "History", "PlayFromHere"));
            }

            var newBoard = BoardFactory.CreateInstance();
            newBoard.FromFen(historyEntry.Fen);

            int gameIdOfNewGame = m_gameManager.CloneBoard(newBoard, opponent, m_identityProvider.CurrentUser, playAsBlack, game, historyEntry.MoveNumber);

            return RedirectToAction("Details", "Board", new { id = gameIdOfNewGame});
        }

        public ActionResult ShowMove(string gameId, string moveNumber)
		{
			int game = Int32.Parse(gameId);
			int move = Int32.Parse(moveNumber);

            var entries = m_gameManager.FindAllMoves(game);

            if (!entries.Any())
            {
                return HttpNotFound("No such game or move");
            }

            ViewBag.MoveNumber = move;
            return View("History", entries);
		}

        [HttpGet]
        public ContentResult Pgn(int id)
        {
            var content = new ContentResult {ContentType = @"text\plain"};

            var entries = m_gameManager.FindAllMoves(id).ToList();
            var numberOfMoves = entries.Count;
            var gameDetails = m_gameManager.FetchGame(id);
            if (!entries.Any())
            {
                content.Content = "NOT FOUND";
                return content;
            }

            var pgnBuilder = new StringBuilder();
            pgnBuilder.AppendFormat("[White \"{0}\"]\r\n", gameDetails.UserProfileWhite.UserName);
            pgnBuilder.AppendFormat("[Black \"{0}\"]\r\n", gameDetails.UserProfileBlack.UserName);
            // TODO: other PGN fields - timelimit, date, venue etc.
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

            if (gameDetails.GameOver)
            {
                if (gameDetails.UserProfileWinner == null)
                {
                    pgnBuilder.Append(" 1/2-1/2");
                }
                else
                {
                    if (gameDetails.UserProfileWhite.UserId == gameDetails.UserProfileWinner.UserId)
                    {
                        pgnBuilder.Append(" 1-0");
                    }

                    if (gameDetails.UserProfileBlack.UserId == gameDetails.UserProfileWinner.UserId)
                    {
                        pgnBuilder.Append(" 0-1");
                    }
                }
            }
            else
            {
                pgnBuilder.Append(" *");
            }

            content.Content = pgnBuilder.ToString();
            return content;
        }
    }
}
