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

		public ActionResult PlayFromHere(string gameId, string moveNumber)
		{
			int game = Int32.Parse(gameId);
			int move = Int32.Parse(moveNumber);
			string opponent = "";

			// Find the history entry for the currently displayed position
			var entry = m_db.HistoryEntries.FirstOrDefault(h => h.GameId == game && h.MoveNumber == move);
			// Find the game in the set of games
			var thisGame = m_db.Boards.FirstOrDefault(b => b.GameId == game);
			bool playAsBlack = false;

            if (entry == null || thisGame == null)
            {
                return View("Error", new HandleErrorInfo(new ArgumentException("Can't find game in database to play from here"), "History", "PlayFromHere"));
            }

				// Find the current user in the user database
				var name = System.Web.HttpContext.Current.User.Identity.Name;
                var myProfile = m_db.UserProfiles.FirstOrDefault(x => x.UserName == name);

                if (myProfile == null) // The user is not logged in or doesn't exist in the database for some reason
                {
                    return View("Error", new HandleErrorInfo(new ArgumentException("User must be logged in"),"History","PlayFromHere"));
                }

				if(thisGame.UserIdWhite == myProfile.UserId)
				{
					opponent = thisGame.UserIdBlack.ToString();
				}
				else if (thisGame.UserIdBlack == myProfile.UserId)
				{
				    opponent = thisGame.UserIdWhite.ToString();
				    playAsBlack = true;
				}

		    var newBoard = new BoardImpl();
			newBoard.FromFen(entry.Fen);

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
