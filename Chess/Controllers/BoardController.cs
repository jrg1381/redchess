﻿using System;
using System.Linq;
using System.Web.Mvc;
using Chess.Filters;
using Microsoft.AspNet.SignalR;
using RedChess.ChessCommon.Enumerations;
using RedChess.EngineFactory;
using RedChess.WebEngine.Repositories;

namespace Chess.Controllers
{
    public interface ICurrentUser
    {
        string CurrentUser { get; }
    }

    public class CurrentUserImpl : ICurrentUser
    {
        public string CurrentUser
        {
            get { return System.Web.HttpContext.Current.User.Identity.Name.ToLowerInvariant(); }
        }
    }

    public class BoardController : Controller
    {
        private readonly IGameManager m_gameManager;
        private readonly UserProfileRepository m_usersRepository = new UserProfileRepository();
        private readonly ICurrentUser m_identityProvider;

        public BoardController() : this(null, null)
        {
            
        }

        public BoardController(IGameManager gameManager = null, ICurrentUser identityProvider = null)
        {
            m_gameManager = gameManager ?? new GameManager();
            m_identityProvider = identityProvider ?? new CurrentUserImpl();
        }

        // GET: /Board/

        public ActionResult Index()
        {
            return View(m_gameManager.FindAll());
        }

        //
        // GET: /Board/Details/5

        public ActionResult Details(int id = 0)
        {
            var board = m_gameManager.FetchGame(id);
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
            return View(m_usersRepository.FindAll().Where(x => x.UserName != m_identityProvider.CurrentUser));
        }

        //
        // POST: /Board/Create

        [HttpPost]
        [ValidateAntiForgeryToken]
        public JsonResult Create(string opponent, string timeLimit, bool useClock = false, bool playAsBlack = false)
        {
            double timeLimitAsNumber = 0;

            if (useClock)
            {
                var validTime = Double.TryParse(timeLimit, out timeLimitAsNumber);
                if (!validTime)
                {
                    return Json(new { success = false, errors = new [] {"Could not parse time limit"} });
                }
                else
                {
                    if (timeLimitAsNumber < 1 || timeLimitAsNumber > 180)
                    {
                        return Json(new { success = false, errors = new [] {"Time limit out of range"} });
                    }
                }
            }
            int opponentId = Int32.Parse(opponent);

            var newBoard = BoardFactory.CreateInstance();
            var newGameId = m_gameManager.Add(newBoard, opponentId, m_identityProvider.CurrentUser, playAsBlack, (int)timeLimitAsNumber * 60 * 1000);

            RefreshIndexPage();
            return Json(new { success = true, redirect = "/Board/Details/" + newGameId });

        }

        private void RefreshIndexPage()
        {
            var jsonObject = Json(new {}); // TODO: Put useful information in here
            IHubContext hubContext = GlobalHost.ConnectionManager.GetHubContext<UpdateServer>();
            hubContext.Clients.Group("IndexWatchers").gameListUpdate(jsonObject);
        }

        //
        // GET: /Board/Delete/5

        public ActionResult Delete(int id = 0)
        {
            var board = m_gameManager.FetchGame(id);
            if (board == null)
            {
                return RedirectToAction("Index");
            }
            return View(board);
        }

        [HttpPost, ActionName("DeleteMultiple")]
        [ValidateAntiForgeryToken]
        [VerifyIsParticipant]
        public ActionResult DeleteMultiple(string ids)
        {
            foreach (var id in ids.Split(',').Select(Int32.Parse))
            {
                DestroyBoard(id);
            }

            return Json(new { success = true, redirect = "/Board/Index" });
        }

        private void DestroyBoard(int id)
        {
            m_gameManager.Delete(id);
        }

        //
        // POST: /Board/Delete/5

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        [VerifyIsParticipant]
        public ActionResult DeleteConfirmed(int id)
        {
            DestroyBoard(id);

            RefreshIndexPage();
            return RedirectToAction("Index");
        }

        private bool IsCurrentUsersTurn(int gameId)
        {
            return m_gameManager.IsUsersTurn(gameId, m_identityProvider.CurrentUser); 
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [VerifyIsParticipant]
        public ActionResult TimedOut(int id, string message, string timedoutcolor)
        {
            var game = m_gameManager.FetchGame(id);

            m_gameManager.TimeGameOut(id, message, timedoutcolor);
            var jsonObject = new { fen = game.Fen, message = message, status = "TIME" };

            IHubContext hubContext = GlobalHost.ConnectionManager.GetHubContext<UpdateServer>();
            hubContext.Clients.Group(game.GameId.ToString()).addMessage(jsonObject);

            return Json(jsonObject);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [VerifyIsParticipant]
        public ActionResult Resign(int id)
        {
            var game = m_gameManager.FetchGame(id);
            var resignationMessage = String.Format("{0} resigned", m_identityProvider.CurrentUser);

            // If current player is white, and is resigning, then black wins
            var winner = game.UserProfileWhite.UserName == m_identityProvider.CurrentUser
                ? game.UserProfileBlack.UserId
                : game.UserProfileWhite.UserId;

            m_gameManager.EndGameWithMessage(id, resignationMessage, winner);

            var jsonObject = new { fen = game.Fen, message = resignationMessage, status = "RESIGN" };

            IHubContext hubContext = GlobalHost.ConnectionManager.GetHubContext<UpdateServer>();
            hubContext.Clients.Group(id.ToString()).addMessage(jsonObject);

            return Json(jsonObject);
        }

        public bool MayManipulateBoard(int gameId)
        {
            return MayManipulateBoard(gameId, m_identityProvider.CurrentUser);
        }

        internal bool MayManipulateBoard(int gameId, string userName)
        {
            var game = m_gameManager.FetchGame(gameId);
            return MayManipulateBoard(game, userName);
        }

        private bool MayManipulateBoard(IGameBinding game, string userName)
        {
            return (game.UserProfileBlack.UserName == userName || game.UserProfileWhite.UserName == userName);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [VerifyIsParticipant]
        public ActionResult ClaimDraw(int id)
        {
            var game = m_gameManager.FetchGame(id);

            if (!game.MayClaimDraw)
            {
                return Json(new {fen = game.Fen, message = "You may not claim a draw in this position", status = "FAIL"});
            }

            m_gameManager.EndGameWithMessage(id, "Draw claimed");

            var jsonObject = new { fen = game.Fen, message = "Draw claimed", status = "DRAW" };

            IHubContext hubContext = GlobalHost.ConnectionManager.GetHubContext<UpdateServer>();
            hubContext.Clients.Group(id.ToString()).addMessage(jsonObject);

            return Json(jsonObject);
        }

        //
		// POST: /Board/PlayMove/5

        [HttpPost]
        [ValidateAntiForgeryToken]
        [VerifyIsParticipant]
        public ActionResult PlayMove(int id, string start, string end, string promote)
        {
            var game = m_gameManager.FetchGame(id);

            if (game == null)
            {
                return Json(new {fen = "PpPpPpPp/pPpPpPpP/PpPpPpPp/pPpPpPpP/PpPpPpPp/pPpPpPpP/PpPpPpPp/pPpPpPpP", message = "This board no longer exists", status = "AUTH"});
            }

            if (game.GameOver)
            {
                return Json(new {fen = game.Fen, message = game.Status, status = "FAIL"});
            }

            // This is also covered by client side validation
            if (!IsCurrentUsersTurn(id))
            {
                return Json(new { fen = game.Fen, message = "It's not your turn.", status = "AUTH" });
            }

            Location endLocation = Location.InvalidSquare, startLocation = Location.InvalidSquare;
            bool parseOk = Enum.TryParse(start, out startLocation) && Enum.TryParse(end, out endLocation);

            if (!parseOk)
            {
                return Json(new { fen = game.Fen, message = "Invalid move", status = "FAIL" });
            }

            bool success = m_gameManager.Move(id, startLocation, endLocation);

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
                m_gameManager.PromotePiece(id, promote);
            }

            m_gameManager.UpdateMessage(id);

            game = m_gameManager.FetchGame(id);
            string messageForUser = game.Status;
            string lastMove = game.LastMove;

            var jsonObject = new { fen = game.Fen, message = messageForUser, lastmove = lastMove, movefrom = start, moveto = end, status = "OK", mayClaimDraw = game.MayClaimDraw };

            IHubContext hubContext = GlobalHost.ConnectionManager.GetHubContext<UpdateServer>();
            hubContext.Clients.Group(id.ToString()).addMessage(jsonObject);

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
