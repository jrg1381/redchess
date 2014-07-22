using System;
using System.Linq;
using System.Web.Mvc;
using Chess.Models;
using Microsoft.AspNet.SignalR;
using RedChess.ChessCommon.Enumerations;
using RedChess.ChessCommon.Interfaces;

namespace Chess.Controllers
{
    public class BoardController : Controller
    {
        private readonly ChessContext m_dbChessContext = new ChessContext();

        // GET: /Board/

        public ActionResult Index()
        {
            return View(m_dbChessContext.Boards);
        }

        //
        // GET: /Board/Details/5

        public ActionResult Details(int id = 0)
        {
            BoardDto board = m_dbChessContext.Boards.Find(id);
            if (board == null)
            {
                return RedirectToAction("Index");
            }
            return View(board);
        }

        //
        // GET: /Board/Create

        public ActionResult Create()
        {
            return View(m_dbChessContext.UserProfiles.Where(profile => profile.UserName != System.Web.HttpContext.Current.User.Identity.Name));
        }

        //
        // POST: /Board/Create

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(BoardImpl board, string opponent, bool useClock, string timeLimit, bool playAsBlack = false)
        {
            if (ModelState.IsValid)
            {
	            int opponentId = Int32.Parse(opponent);
	            var name = System.Web.HttpContext.Current.User.Identity.Name;
                var myProfile = m_dbChessContext.UserProfiles.First(x => x.UserName == name);
				var dto = playAsBlack ? new BoardDto(board, opponentId, myProfile.UserId) : new BoardDto(board, myProfile.UserId, opponentId);
	            m_dbChessContext.Boards.Add(dto);
	            m_dbChessContext.SaveChanges();

				if (useClock)
				{
					double timeLimitAsNumber = 0;
					Double.TryParse(timeLimit, out timeLimitAsNumber);
					var clock = new Clock(dto.GameId, (int)(timeLimitAsNumber*60*1000));
					m_dbChessContext.Clocks.Add(clock);
				}

                dto.UpdateMessage();
	            m_dbChessContext.SaveChanges();
                m_dbChessContext.HistoryEntries.Add(new HistoryEntry() { GameId = dto.GameId, Fen = dto.Fen, MoveNumber = 1 });
                m_dbChessContext.SaveChanges();
				return RedirectToAction("Details", "Board", new {id = dto.GameId});
            }

            return RedirectToAction("Index");
        }

	    //
        // GET: /Board/Delete/5

        public ActionResult Delete(int id = 0)
        {
			BoardDto board = m_dbChessContext.Boards.Find(id);
            if (board == null)
            {
                return RedirectToAction("Index");
            }

			return View(board);
        }

        //
        // POST: /Board/Delete/5

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
			BoardDto board = m_dbChessContext.Boards.Find(id);

            if (board == null || !MayManipulateBoard(board)) return RedirectToAction("Index");
            
            Clock clock = m_dbChessContext.Clocks.FirstOrDefault(c => c.GameId == id);

            if (clock != null)
            {
                m_dbChessContext.Clocks.Remove(clock);
            }

            // TODO: Do this via cascading delete in the database itself instead of the application
            Array.ForEach(m_dbChessContext.HistoryEntries.Where(x => x.GameId == board.GameId).ToArray(), entry => m_dbChessContext.HistoryEntries.Remove(entry));

            m_dbChessContext.Boards.Remove(board);
            m_dbChessContext.SaveChanges();

            return RedirectToAction("Index");
        }

		private bool MayManipulateBoard(BoardDto dto)
		{
			var name = System.Web.HttpContext.Current.User.Identity.Name;
            var profile = m_dbChessContext.UserProfiles.FirstOrDefault(x => x.UserName == name);

			return (profile != null && (profile.UserId == dto.UserIdWhite || profile.UserId == dto.UserIdBlack));
		}

        private bool IsCurrentUsersTurn(BoardDto dto)
        {
            var name = System.Web.HttpContext.Current.User.Identity.Name;
            var profile = m_dbChessContext.UserProfiles.FirstOrDefault(x => x.UserName == name);
            if (profile == null)
                return false;
            return dto.IsUsersTurn(profile.UserId);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult TimedOut(int id, string message)
        {
            BoardDto board = m_dbChessContext.Boards.Find(id);

            if (!MayManipulateBoard(board))
            {
                return Json(new { fen = board.Fen, message = "You are not allowed to play on this board", status = "AUTH" });
            }

            // Because the user who timed out hasn't made a move to update the clock, the database will contains an elapsed time
            // which is too small. It will be equal to the time spent on all their _completed_ moves, and not the time they spent
            // thinking about the last one. This has the annoying effect that reloading the page will increase their time from 0.
            // To fix this, edit the loser's elapsed time in the database and set it equal to the time limit for the game.

            int timeLimit = board.Clock().TimeLimitMs;
            var name = System.Web.HttpContext.Current.User.Identity.Name;
            var profile = m_dbChessContext.UserProfiles.FirstOrDefault(x => x.UserName == name);

            if (profile.UserId == board.UserIdWhite)
            {
                m_dbChessContext.Clocks.Single(clock => clock.GameId == id).TimeElapsedWhiteMs = timeLimit;
            }
            else if (profile.UserId == board.UserIdBlack)
            {
                m_dbChessContext.Clocks.Single(clock => clock.GameId == id).TimeElapsedBlackMs = timeLimit;
            }

            board.EndGameWithMessage(message);
            m_dbChessContext.SaveChanges();

            var jsonObject = new { fen = board.Fen, message = message, status = "TIME" };

            IHubContext hubContext = GlobalHost.ConnectionManager.GetHubContext<UpdateServer>();
            hubContext.Clients.Group(board.GameId.ToString()).addMessage(jsonObject);

            return Json(jsonObject);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Resign(int id)
        {
            BoardDto board = m_dbChessContext.Boards.Find(id);

            if (!MayManipulateBoard(board))
            {
                return Json(new { fen = board.Fen, message = "You are not allowed to play on this board", status = "AUTH" });
            }
            
            var name = System.Web.HttpContext.Current.User.Identity.Name;
            var profile = m_dbChessContext.UserProfiles.FirstOrDefault(x => x.UserName == name);
            var resignationMessage = String.Format("{0} resigned", profile.UserName);

            board.EndGameWithMessage(resignationMessage);
            m_dbChessContext.SaveChanges();

            var jsonObject = new { fen = board.Fen, message = resignationMessage, status = "RESIGN" };

            IHubContext hubContext = GlobalHost.ConnectionManager.GetHubContext<UpdateServer>();
            hubContext.Clients.Group(board.GameId.ToString()).addMessage(jsonObject);

            return Json(jsonObject);
        }

		//
		// POST: /Board/PlayMove/5

		[HttpPost]
		[ValidateAntiForgeryToken]
		public ActionResult PlayMove(int id, string start, string end, string promote)
		{
			BoardDto board = m_dbChessContext.Boards.Find(id);
            // Allowed to be null for an un-timed game
			var clock = m_dbChessContext.Clocks.FirstOrDefault(c => c.GameId == id);

            // This is also covered by client side validation
			if(!MayManipulateBoard(board))
			{
				return Json(new { fen = board.Fen, message = "You are not allowed to play on this board", status = "AUTH" });
			}

            // This is also covered by client side validation
			if(!IsCurrentUsersTurn(board))
			{
				return Json(new { fen = board.Fen, message = "It's not your turn.", status = "AUTH" });
			}

            Location endLocation = Location.InvalidSquare, startLocation = Location.InvalidSquare;
            bool parseOk = Enum.TryParse<Location>(start, out startLocation) && Enum.TryParse<Location>(end, out endLocation);

            if (!parseOk)
            {
                return Json(new { fen = board.Fen, message = "Invalid move", status = "FAIL" });
            }

            bool success = board.Move(startLocation, endLocation);
			
			if(!success)
			{
				string errorMessage = "Invalid move";

				if(!String.IsNullOrEmpty(board.Status))
				{
					errorMessage = board.Status + " - invalid move";
				}

				return Json(new { fen = board.Fen, message = errorMessage, status = "FAIL" });
			}

            int nextMoveNumber = m_dbChessContext.HistoryEntries.Where(x => x.GameId == board.GameId).Max(x => x.MoveNumber) + 1;
            m_dbChessContext.HistoryEntries.Add(new HistoryEntry() { Fen = board.Fen, GameId = board.GameId, MoveNumber = nextMoveNumber} );

			if (!String.IsNullOrEmpty(promote))
			{
				board.PromotePiece(promote);
			}

			if (clock != null)
			{
				if (board.Turn == "Black")
				{
					clock.LastActionBlack = DateTime.UtcNow;
					clock.TimeElapsedWhiteMs += (int)(DateTime.UtcNow - clock.LastActionWhite).TotalMilliseconds;
				}
				if (board.Turn == "White")
				{
					clock.LastActionWhite = DateTime.UtcNow;
					clock.TimeElapsedBlackMs += (int)(DateTime.UtcNow - clock.LastActionBlack).TotalMilliseconds;
				}
			}

			board.UpdateMessage();
			m_dbChessContext.SaveChanges();

            string messageForUser = board.Status;
			var jsonObject = new {fen = board.Fen, message = messageForUser, movefrom = start, moveto = end, status = "OK"};

			IHubContext hubContext = GlobalHost.ConnectionManager.GetHubContext<UpdateServer>();
			hubContext.Clients.Group(board.GameId.ToString()).addMessage(jsonObject);
			return Json(jsonObject);			
		}

        protected override void Dispose(bool disposing)
        {
            m_dbChessContext.Dispose();
            base.Dispose(disposing);
        }
    }

	public class UpdateServer : Hub
	{
		public void Join(string groupName)
		{
			Groups.Add(Context.ConnectionId, groupName);
		}
	}
}