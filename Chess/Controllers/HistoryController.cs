using System;
using System.Linq;
using System.Web.Mvc;
using Chess.Models;
using RedChess.ChessCommon.Interfaces;
using RedChess.EngineFactory;

namespace Chess.Controllers
{
    public class HistoryController : Controller
    {
		private readonly ChessContext m_db = new ChessContext();

        //
        // GET: /History/

        public ActionResult Index()
        {
			return HttpNotFound();
        }

        public ActionResult PlayFromHere(string move, string gameId)
		{
		    bool playAsBlack = false;
		    string opponent = "";
            int moveNumber = Int32.Parse(move);
            int game = Int32.Parse(gameId);

				// Find the current user in the user database
		    var name = System.Web.HttpContext.Current.User.Identity.Name;
		    var myProfile = m_db.UserProfiles.FirstOrDefault(x => x.UserName == name);

		    if (myProfile == null) // The user is not logged in or doesn't exist in the database for some reason
		    {
		        return View("Error", new HandleErrorInfo(new ArgumentException("User must be logged in"), "History", "PlayFromHere"));
		    }

		    var thisGame = m_db.Boards.FirstOrDefault(x => x.GameId == game);
		    if (thisGame == null)
		    {
                return View("Error", new HandleErrorInfo(new ArgumentException("Source game not found in database"), "History", "PlayFromHere"));
		    }

            if (thisGame.UserIdWhite == myProfile.UserId)
		    {
		        opponent = thisGame.UserIdBlack.ToString();
		    }
		    else if (thisGame.UserIdBlack == myProfile.UserId)
		    {
		        opponent = thisGame.UserIdWhite.ToString();
		        playAsBlack = true;
		    }

            var historyEntry = m_db.HistoryEntries.FirstOrDefault(x => x.GameId == game && x.MoveNumber == moveNumber + 1);
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

			var entries = m_db.HistoryEntries.Where(h => h.GameId == game);

            if (!entries.Any())
			{
				return HttpNotFound("No such game or move");
			}           

            ViewBag.MoveNumber = move;
            return View("History", entries);
		}
    }
}
