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
        private GameRepository m_repository = new GameRepository();
        private ClockRepository m_clockRepository = new ClockRepository();

        // GET: /Board/

        public ActionResult Index()
        {
            return View(m_repository.FindAll());
        }

        //
        // GET: /Board/Details/5

        public ActionResult Details(int id = 0)
        {
            var board = m_repository.FindById(id);
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
            using (var dbChessContext = new ChessContext())
            {
                return View(dbChessContext.UserProfiles.Where(profile => profile.UserName != System.Web.HttpContext.Current.User.Identity.Name).ToList());
            }
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
                var dto = m_repository.Add(board, opponentId, System.Web.HttpContext.Current.User.Identity.Name, playAsBlack);

                if (useClock)
                {
                    double timeLimitAsNumber = 0;
                    Double.TryParse(timeLimit, out timeLimitAsNumber);
                    m_clockRepository.AddClock(dto.Id, (int) (timeLimitAsNumber*60*1000));
                }

                return RedirectToAction("Details", "Board", new {id = dto.Id});
            }

            return RedirectToAction("Index");
        }

        //
        // GET: /Board/Delete/5

        public ActionResult Delete(int id = 0)
        {
            var board = m_repository.FindById(id);

            if (board == null)
            {
                return RedirectToAction("Index");
            }

            return View(board);
        }

        [HttpPost, ActionName("DeleteMultiple")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteMultiple(string ids)
        {
            foreach (var id in ids.Split(',').Select(Int32.Parse))
            {
                DestroyBoard(id);
            }

            return RedirectToAction("Index");
        }

        private void DestroyBoard(int id)
        {
            m_repository.Delete(id);
        }

        //
        // POST: /Board/Delete/5

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
                DestroyBoard(id);
                return RedirectToAction("Index");
        }

        private bool IsCurrentUsersTurn(int gameId)
        {
            return m_repository.FindById(gameId).IsUsersTurn(System.Web.HttpContext.Current.User.Identity.Name);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult TimedOut(int id, string message)
        {
            var game = m_repository.FindById(id);

            if (!m_repository.MayManipulateBoard(id, System.Web.HttpContext.Current.User.Identity.Name))
            {
                return Json(new {fen = game.Fen, message = "You are not allowed to play on this board", status = "AUTH"});
            }

            m_repository.TimeGameOut(id, message, System.Web.HttpContext.Current.User.Identity.Name);
            var jsonObject = new { fen = game.Fen, message = message, status = "TIME" };

            IHubContext hubContext = GlobalHost.ConnectionManager.GetHubContext<UpdateServer>();
            hubContext.Clients.Group(game.Id.ToString()).addMessage(jsonObject);

            return Json(jsonObject);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Resign(int id)
        {
            var game = m_repository.FindById(id);

            if (!m_repository.MayManipulateBoard(id, System.Web.HttpContext.Current.User.Identity.Name))
            {
                return Json(new {fen = game.Fen, message = "You are not allowed to play on this board", status = "AUTH"});
            }

            var resignationMessage = String.Format("{0} resigned", System.Web.HttpContext.Current.User.Identity.Name);

            game.EndGameWithMessage(game.Id, resignationMessage);

            var jsonObject = new { fen = game.Fen, message = resignationMessage, status = "RESIGN" };

            IHubContext hubContext = GlobalHost.ConnectionManager.GetHubContext<UpdateServer>();
            hubContext.Clients.Group(game.Id.ToString()).addMessage(jsonObject);

            return Json(jsonObject);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult ClaimDraw(int id)
        {
            var game = m_repository.FindById(id);

            if (!m_repository.MayManipulateBoard(id, System.Web.HttpContext.Current.User.Identity.Name))
            {
                return Json(new {fen = game.Fen, message = "You are not allowed to play on this board", status = "AUTH"});
            }

            if (!game.MayClaimDraw)
            {
                return Json(new {fen = game.Fen, message = "You may not claim a draw in this position", status = "FAIL"});
            }

            game.EndGameWithMessage(id, "Draw claimed");

            var jsonObject = new { fen = game.Fen, message = "Draw claimed", status = "DRAW" };

            IHubContext hubContext = GlobalHost.ConnectionManager.GetHubContext<UpdateServer>();
            hubContext.Clients.Group(game.Id.ToString()).addMessage(jsonObject);

            return Json(jsonObject);
        }

        //
		// POST: /Board/PlayMove/5

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult PlayMove(int id, string start, string end, string promote)
        {
            var game = m_repository.FindById(id);

            if (game == null)
            {
                return Json(new {fen = "PpPpPpPp/pPpPpPpP/PpPpPpPp/pPpPpPpP/PpPpPpPp/pPpPpPpP/PpPpPpPp/pPpPpPpP", message = "This board no longer exists", status = "AUTH"});
            }

            if (!m_repository.MayManipulateBoard(id, System.Web.HttpContext.Current.User.Identity.Name))
            {
                return Json(new {fen = game.Fen, message = "You are not allowed to play on this board", status = "AUTH"});
            }

            // This is also covered by client side validation
            if (!IsCurrentUsersTurn(game.Id))
            {
                return Json(new { fen = game.Fen, message = "It's not your turn.", status = "AUTH" });
            }

            Location endLocation = Location.InvalidSquare, startLocation = Location.InvalidSquare;
            bool parseOk = Enum.TryParse(start, out startLocation) && Enum.TryParse(end, out endLocation);

            if (!parseOk)
            {
                return Json(new { fen = game.Fen, message = "Invalid move", status = "FAIL" });
            }

            bool success = game.Move(startLocation, endLocation);

            if (!success)
            {
                string errorMessage = "Invalid move";

                if (!String.IsNullOrEmpty(game.Status))
                {
                    errorMessage = game.Status + " - invalid move";
                }

                return Json(new { fen = game.Fen, message = errorMessage, status = "FAIL" });
            }

            if (!String.IsNullOrEmpty(promote))
            {
                game.PromotePiece(promote);
            }

            game.UpdateMessage(game.Id);

            string messageForUser = game.Status;
            string lastMove = game.LastMove;

            var jsonObject = new { fen = game.Fen, message = messageForUser, lastmove = lastMove, movefrom = start, moveto = end, status = "OK", mayClaimDraw = game.MayClaimDraw };

            IHubContext hubContext = GlobalHost.ConnectionManager.GetHubContext<UpdateServer>();
            hubContext.Clients.Group(game.Id.ToString()).addMessage(jsonObject);
            return Json(jsonObject);
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
