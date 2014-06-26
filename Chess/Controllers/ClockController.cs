using System;
using System.Linq;
using System.Web.Mvc;
using Chess.Models;
using Microsoft.AspNet.SignalR;

namespace Chess.Controllers
{
    public class ClockController : Controller
    {
        private readonly ChessContext db = new ChessContext();

		[System.Web.Mvc.HttpPost]
		public ActionResult PlayerReady(int id)
		{
			var board = db.Boards.Find(id);
			var clock = db.Clocks.FirstOrDefault(c => c.GameId == id);

			if (board == null || clock == null)
				return Json(new { status = "NULL" });

			string playerColor = board.CurrentPlayerColor(System.Web.HttpContext.Current.User.Identity.Name);
			//var transactionOptions = new TransactionOptions { IsolationLevel = IsolationLevel.Serializable, Timeout = TimeSpan.FromSeconds(10) };

			// Need to avoid the race condition where white and black are accessing this function at the same time.
			//using (var scope = new TransactionScope(TransactionScopeOption.Required, transactionOptions))
			//{
				switch (playerColor)
				{
					case "w":
						clock.PlayersReady |= 1;
						break;
					case "b":
						clock.PlayersReady |= 2;
						break;
					default:
						return Json(new {status = "AUTH"});
				}

				db.SaveChanges();
			//	scope.Complete();
			//}

			string status = "WAIT";

			if (clock.PlayersReady == 3)
			{
				clock.LastActionWhite = DateTime.UtcNow;
				db.SaveChanges();
				status = "OK";
			}

			IHubContext hubContext = GlobalHost.ConnectionManager.GetHubContext<UpdateServer>();
			hubContext.Clients.Group(board.GameId.ToString()).startClock(new { status = status, who = playerColor });
			return Json(new { status = status });
		}

		[System.Web.Mvc.HttpPost]
		public ActionResult RefreshClock(int id)
		{
			var clock = db.Clocks.FirstOrDefault(c => c.GameId == id);
			if (clock == null)
				return Json(new { status = "NULL" });

			int timeLeftBlack = clock.TimeLimitMs - clock.TimeElapsedBlackMs;
			int timeLeftWhite = clock.TimeLimitMs - clock.TimeElapsedWhiteMs;

			return Json(new { timeleftblack = timeLeftBlack, timeleftwhite = timeLeftWhite, status = "OK" });
		}

        protected override void Dispose(bool disposing)
        {
            db.Dispose();
            base.Dispose(disposing);
        }
    }
}