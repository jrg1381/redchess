using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Http.Results;
using System.Web.Mvc;
using Chess.Models;
using Microsoft.AspNet.SignalR;
using RedChess.ChessCommon.Enumerations;
using RedChess.ChessCommon.Interfaces;

namespace Chess.Controllers
{
    public class BoardController : Controller
    {
        // GET: /Board/

        public ActionResult Index()
        {
            using (var dbChessContext = new ChessContext())
            {
                return View(dbChessContext.Boards.ToList());
            }
        }

        //
        // GET: /Board/Details/5

        public ActionResult Details(int id = 0)
        {
            using (var dbChessContext = new ChessContext())
            {
                Game board = dbChessContext.Boards.Find(id);
                if (board == null)
                {
                    return RedirectToAction("Index");
                }
                return View(board);
            }
        }

        //
        // GET: /Board/Create

        public ActionResult Create()
        {
            using (var dbChessContext = new ChessContext())
            {
                return View(dbChessContext.UserProfiles.Where(profile => profile.UserName != System.Web.HttpContext.Current.User.Identity.Name));
            }
        }

        //
        // POST: /Board/Create

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(BoardImpl board, string opponent, bool useClock, string timeLimit, bool playAsBlack = false)
        {
            using (var dbChessContext = new ChessContext())
            {
                if (ModelState.IsValid)
                {
                    int opponentId = Int32.Parse(opponent);
                    var myProfile = UserUtilities.UserProfileFromName(dbChessContext);
                    var dto = playAsBlack ? new Game(board, opponentId, myProfile.UserId) : new Game(board, myProfile.UserId, opponentId);
                    dbChessContext.Boards.Add(dto);
                    dbChessContext.SaveChanges();

                    if (useClock)
                    {
                        double timeLimitAsNumber = 0;
                        Double.TryParse(timeLimit, out timeLimitAsNumber);
                        var clock = new Clock(dto.GameId, (int) (timeLimitAsNumber*60*1000));
                        dbChessContext.Clocks.Add(clock);
                    }

                    dbChessContext.HistoryEntries.Add(new HistoryEntry() { GameId = dto.GameId, Fen = dto.Fen, MoveNumber = 1, Move = "" });
                    dbChessContext.SaveChanges();
                    return RedirectToAction("Details", "Board", new {id = dto.GameId});
                }

                return RedirectToAction("Index");
            }
        }

	    //
        // GET: /Board/Delete/5

        public ActionResult Delete(int id = 0)
        {
            using (var dbChessContext = new ChessContext())
            {
                Game board = dbChessContext.Boards.Find(id);
                if (board == null)
                {
                    return RedirectToAction("Index");
                }

                return View(board);
            }
        }

        [HttpPost, ActionName("DeleteMultiple")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteMultiple(string ids)
        {
            using (var dbChessContext = new ChessContext())
            {
                foreach (var id in ids.Split(',').Select(Int32.Parse))
                    DestroyBoard(id, dbChessContext);

                dbChessContext.SaveChanges();
                return RedirectToAction("Index");
            }
        }

        private void DestroyBoard(int id, ChessContext context)
        {

            var board = context.Boards.Find(id);

            if (board != null && MayManipulateBoard(board))
            {
                context.Boards.Remove(board);
            }
        }

        //
        // POST: /Board/Delete/5

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            using (var dbChessContext = new ChessContext())
            {
                DestroyBoard(id, dbChessContext);

                dbChessContext.SaveChanges();
                return RedirectToAction("Index");
            }
        }

		private bool MayManipulateBoard(Game dto)
		{
		    using (var dbChessContext = new ChessContext())
		    {
                var profile = UserUtilities.UserProfileFromName(dbChessContext);

		        return (profile != null && (profile.UserId == dto.UserIdWhite || profile.UserId == dto.UserIdBlack));
		    }
		}

        private bool IsCurrentUsersTurn(Game dto)
        {
            using (var dbChessContext = new ChessContext())
            {
                var profile = UserUtilities.UserProfileFromName(dbChessContext);
                if (profile == null)
                    return false;
                return dto.IsUsersTurn(profile.UserId);
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult TimedOut(int id, string message)
        {
            using (var dbChessContext = new ChessContext())
            {
                Game board = dbChessContext.Boards.Find(id);

                if (!MayManipulateBoard(board))
                {
                    return Json(new {fen = board.Fen, message = "You are not allowed to play on this board", status = "AUTH"});
                }

                // Because the user who timed out hasn't made a move to update the clock, the database will contains an elapsed time
                // which is too small. It will be equal to the time spent on all their _completed_ moves, and not the time they spent
                // thinking about the last one. This has the annoying effect that reloading the page will increase their time from 0.
                // To fix this, edit the loser's elapsed time in the database and set it equal to the time limit for the game.

                int timeLimit = board.Clock().TimeLimitMs;
                var profile = UserUtilities.UserProfileFromName(dbChessContext);

                if (profile.UserId == board.UserIdWhite)
                {
                    dbChessContext.Clocks.Single(clock => clock.GameId == id).TimeElapsedWhiteMs = timeLimit;
                }
                else if (profile.UserId == board.UserIdBlack)
                {
                    dbChessContext.Clocks.Single(clock => clock.GameId == id).TimeElapsedBlackMs = timeLimit;
                }

                board.EndGameWithMessage(message);
                dbChessContext.SaveChanges();

                var jsonObject = new {fen = board.Fen, message = message, status = "TIME"};

                IHubContext hubContext = GlobalHost.ConnectionManager.GetHubContext<UpdateServer>();
                hubContext.Clients.Group(board.GameId.ToString()).addMessage(jsonObject);

                return Json(jsonObject);
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Resign(int id)
        {
            using (var dbChessContext = new ChessContext())
            {
                Game board = dbChessContext.Boards.Find(id);

                if (!MayManipulateBoard(board))
                {
                    return Json(new {fen = board.Fen, message = "You are not allowed to play on this board", status = "AUTH"});
                }

                var profile = UserUtilities.UserProfileFromName(dbChessContext);
                var resignationMessage = String.Format("{0} resigned", profile.UserName);

                board.EndGameWithMessage(resignationMessage);
                dbChessContext.SaveChanges();

                var jsonObject = new {fen = board.Fen, message = resignationMessage, status = "RESIGN"};

                IHubContext hubContext = GlobalHost.ConnectionManager.GetHubContext<UpdateServer>();
                hubContext.Clients.Group(board.GameId.ToString()).addMessage(jsonObject);

                return Json(jsonObject);
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult ClaimDraw(int id)
        {
            using (var dbChessContext = new ChessContext())
            {
                Game board = dbChessContext.Boards.Find(id);

                if (!MayManipulateBoard(board))
                {
                    return Json(new {fen = board.Fen, message = "You are not allowed to play on this board", status = "AUTH"});
                }

                if (!board.MayClaimDraw)
                {
                    return Json(new {fen = board.Fen, message = "You may not claim a draw in this position", status = "FAIL"});
                }

                board.EndGameWithMessage("Draw claimed");
                dbChessContext.SaveChanges();

                var jsonObject = new {fen = board.Fen, message = "Draw claimed", status = "DRAW"};

                IHubContext hubContext = GlobalHost.ConnectionManager.GetHubContext<UpdateServer>();
                hubContext.Clients.Group(board.GameId.ToString()).addMessage(jsonObject);

                return Json(jsonObject);
            }
        }

		//
		// POST: /Board/PlayMove/5

		[HttpPost]
		[ValidateAntiForgeryToken]
		public ActionResult PlayMove(int id, string start, string end, string promote)
		{
		    using (var dbChessContext = new ChessContext())
		    {
                Game board = dbChessContext.Boards.Find(id);
		        if (board == null)
		        {
		            return Json(new {fen = "PpPpPpPp/pPpPpPpP/PpPpPpPp/pPpPpPpP/PpPpPpPp/pPpPpPpP/PpPpPpPp/pPpPpPpP", message = "This board no longer exists", status = "AUTH"});
		        }
		        // Allowed to be null for an un-timed game
                var clock = dbChessContext.Clocks.FirstOrDefault(c => c.GameId == id);

		        // This is also covered by client side validation
		        if (!MayManipulateBoard(board))
		        {
		            return Json(new {fen = board.Fen, message = "You are not allowed to play on this board", status = "AUTH"});
		        }

		        // This is also covered by client side validation
		        if (!IsCurrentUsersTurn(board))
		        {
		            return Json(new {fen = board.Fen, message = "It's not your turn.", status = "AUTH"});
		        }

		        Location endLocation = Location.InvalidSquare, startLocation = Location.InvalidSquare;
		        bool parseOk = Enum.TryParse(start, out startLocation) && Enum.TryParse(end, out endLocation);

		        if (!parseOk)
		        {
		            return Json(new {fen = board.Fen, message = "Invalid move", status = "FAIL"});
		        }

		        bool success = board.Move(startLocation, endLocation);

		        if (!success)
		        {
		            string errorMessage = "Invalid move";

		            if (!String.IsNullOrEmpty(board.Status))
		            {
		                errorMessage = board.Status + " - invalid move";
		            }

		            return Json(new {fen = board.Fen, message = errorMessage, status = "FAIL"});
		        }

		        if (!String.IsNullOrEmpty(promote))
		        {
		            board.PromotePiece(promote);
		        }

                int nextMoveNumber = dbChessContext.HistoryEntries.Where(x => x.GameId == board.GameId).Max(x => x.MoveNumber) + 1;
                dbChessContext.HistoryEntries.Add(new HistoryEntry() { Fen = board.Fen, GameId = board.GameId, MoveNumber = nextMoveNumber, Move = board.LastMove });

		        if (clock != null)
		        {
		            if (board.Turn == "Black")
		            {
		                clock.LastActionBlack = DateTime.UtcNow;
		                clock.TimeElapsedWhiteMs += (int) (DateTime.UtcNow - clock.LastActionWhite).TotalMilliseconds;
		            }
		            if (board.Turn == "White")
		            {
		                clock.LastActionWhite = DateTime.UtcNow;
		                clock.TimeElapsedBlackMs += (int) (DateTime.UtcNow - clock.LastActionBlack).TotalMilliseconds;
		            }
		        }

		        board.UpdateMessage();
                dbChessContext.SaveChanges();

		        string messageForUser = board.Status;
		        string lastMove = board.LastMove;

		        var jsonObject = new {fen = board.Fen, message = messageForUser, lastmove = lastMove, movefrom = start, moveto = end, status = "OK", mayClaimDraw = board.MayClaimDraw};

		        IHubContext hubContext = GlobalHost.ConnectionManager.GetHubContext<UpdateServer>();
		        hubContext.Clients.Group(board.GameId.ToString()).addMessage(jsonObject);
		        return Json(jsonObject);
		    }
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
