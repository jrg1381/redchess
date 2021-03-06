﻿using System;
using System.Linq;
using System.Web.Mvc;
using Chess.Filters;
using Microsoft.AspNet.SignalR;
using RedChess.ChessCommon.Enumerations;
using RedChess.EngineFactory;
using RedChess.WebEngine;
using RedChess.WebEngine.Repositories;
using RedChess.WebEngine.Repositories.Interfaces;

namespace Chess.Controllers
{
    public class BoardController : Controller
    {
        private readonly IGameManager m_GameManager;
        private readonly ICurrentUser m_IdentityProvider;
        private readonly IDateTimeProvider m_DateTimeProvider;

        public BoardController() : this(null, null, null)
        {
            
        }

        public BoardController(IGameManager gameManager = null, ICurrentUser identityProvider = null, IDateTimeProvider dateTimeProvider = null)
        {
            m_GameManager = gameManager ?? new GameManager();
            m_IdentityProvider = identityProvider ?? new CurrentUserProvider();
            m_DateTimeProvider = dateTimeProvider ?? new DefaultDateTimeProvider();
        }

        // GET: /Board/

        public ActionResult Index()
        {
            // The model is not used in this view
            return View(new object());
        }

        //
        // GET: /Board/Details/5

        public ActionResult Details(int id = 0)
        {
            var board = m_GameManager.FetchGame(id);
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
            return View(m_GameManager.AllUserProfiles().Where(x => x.UserId != m_IdentityProvider.CurrentUserId));
        }


        private JsonResult NewAnalysisBoard()
        {
            var newBoard = BoardFactory.CreateInstance();
            var newGameIdAnalysis = m_GameManager.Add(newBoard, m_IdentityProvider.CurrentUser);
            RefreshIndexPage();
            return Json(new { success = true, redirect = "/Board/Details/" + newGameIdAnalysis });
        }
        //
        // POST: /Board/Create

        [HttpPost]
        [ValidateAntiForgeryToken]
        public JsonResult Create(string opponent, string timeLimit, bool useClock = false, bool playAsBlack = false, bool analysisBoard = false)
        {
            double timeLimitAsNumber = 0;

            if (analysisBoard)
            {
                return NewAnalysisBoard();
            }

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
            var newGameId = m_GameManager.Add(newBoard, opponentId, m_IdentityProvider.CurrentUser, playAsBlack, (int)timeLimitAsNumber * 60 * 1000);

            RefreshIndexPage();
            return Json(new { success = true, redirect = "/Board/Details/" + newGameId });

        }

        private void RefreshIndexPage()
        {
            var jsonObject = Json(new {}); // TODO: Put useful information in here
            IHubContext hubContext = GlobalHost.ConnectionManager.GetHubContext<UpdateServer>();
            hubContext.Clients.Group("IndexWatchers").gameListUpdate(jsonObject);
        }

        [HttpPost, ActionName("OfferDraw")]
        [ValidateAntiForgeryToken]
        [VerifyIsParticipant]
        public ActionResult OfferDraw(int id)
        {
            var game = m_GameManager.FetchGame(id);
            var offerFrom = game.CurrentPlayerColor(m_IdentityProvider.CurrentUser);

            var jsonObject = Json(new { DrawOfferedBy = offerFrom });
            IHubContext hubContext = GlobalHost.ConnectionManager.GetHubContext<UpdateServer>();
            hubContext.Clients.Group(id.ToString()).showDrawOffer(jsonObject);

            return Json(new { success = true });
        }

        public ActionResult Delete(int id = 0)
        {
            var board = m_GameManager.FetchGame(id);
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
            m_GameManager.Delete(id);
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

        private bool IsCurrentUsersTurn(IGameBinding game)
        {
            return m_GameManager.IsUsersTurn(game, m_IdentityProvider.CurrentUser); 
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [VerifyIsParticipant]
        public ActionResult TimedOut(int id, string message, string timedoutcolor)
        {
            var game = m_GameManager.FetchGame(id);

            m_GameManager.TimeGameOut(id, message, timedoutcolor);
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
            var game = m_GameManager.FetchGame(id);
            var resignationMessage = String.Format("{0} resigned", m_IdentityProvider.CurrentUser);

            // If current player is white, and is resigning, then black wins
            var winner = game.UserProfileWhite.UserName == m_IdentityProvider.CurrentUser
                ? game.UserProfileBlack.UserId
                : game.UserProfileWhite.UserId;

            m_GameManager.EndGameWithMessage(id, resignationMessage, winner);

            var jsonObject = new { fen = game.Fen, message = resignationMessage, status = "RESIGN" };

            IHubContext hubContext = GlobalHost.ConnectionManager.GetHubContext<UpdateServer>();
            hubContext.Clients.Group(id.ToString()).addMessage(jsonObject);

            return Json(jsonObject);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [VerifyIsParticipant]
        public ActionResult AgreeDraw(int id, bool offerAccepted)
        {
            var game = m_GameManager.FetchGame(id);
            object jsonObject;

            if (offerAccepted)
            {
                // TODO: A mechanism to prove that the offer was really made in the first place and to
                // TODO: persist the offer in the case that the page is refreshed
                m_GameManager.EndGameWithMessage(id, "Draw agreed");

                jsonObject = new {fen = game.Fen, message = "Draw agreed", status = "DRAW"};
            }
            else
            {
                jsonObject = new { fen = game.Fen, message = "Offer rejected", status = "REJECT", lastmove = game.LastMove, mayClaimDraw = game.MayClaimDraw };
            }

            IHubContext hubContext = GlobalHost.ConnectionManager.GetHubContext<UpdateServer>();
            hubContext.Clients.Group(id.ToString()).addMessage(jsonObject);
            return Json(jsonObject);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [VerifyIsParticipant]
        public ActionResult ClaimDraw(int id)
        {
            var game = m_GameManager.FetchGame(id);

            if (!game.MayClaimDraw)
            {
                return Json(new {fen = game.Fen, message = "You may not claim a draw in this position", status = "FAIL"});
            }

            m_GameManager.EndGameWithMessage(id, "Draw claimed");

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
            // Be generous to the user and take the time as soon as possible, so they lose the least clock time
            var now = m_DateTimeProvider.UtcNow;

            var game = m_GameManager.FetchGame(id);

            if (game == null)
            {
                return Json(new {fen = "PpPpPpPp/pPpPpPpP/PpPpPpPp/pPpPpPpP/PpPpPpPp/pPpPpPpP/PpPpPpPp/pPpPpPpP", message = "This board no longer exists", status = "AUTH"});
            }

            if (game.GameOver)
            {
                return Json(new {fen = game.Fen, message = game.Status, status = "FAIL"});
            }

            // This is also covered by client side validation
            if (!IsCurrentUsersTurn(game))
            {
                return Json(new { fen = game.Fen, message = "It's not your turn.", status = "AUTH" });
            }

            Location endLocation = Location.InvalidSquare, startLocation = Location.InvalidSquare;
            bool parseOk = Enum.TryParse(start, out startLocation) && Enum.TryParse(end, out endLocation);

            if (!parseOk)
            {
                return Json(new { fen = game.Fen, message = "Invalid move", status = "FAIL" });
            }

            GameDto newDto;
            bool success = m_GameManager.Move(id, startLocation, endLocation, promote, now, out newDto);

            if (!success)
            {
                string errorMessage = "Invalid move";

                if (!String.IsNullOrEmpty(game.Status))
                {
                    errorMessage = game.Status + " - invalid move";
                }

                return Json(new { fen = game.Fen, message = errorMessage, status = "FAIL" });
            }

            if (newDto != null) // failure, so keep game the same
            {
                game = new GameBinding(newDto, m_GameManager); // Don't fetch from the DB again
            }

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
