using System.Web.Mvc;
using Chess.Filters;
using RedChess.WebEngine.Repositories;
using Microsoft.AspNet.SignalR;
using RedChess.WebEngine.Repositories.Interfaces;

namespace Chess.Controllers
{
    public class ClockController : Controller
    {
        private readonly IGameManager m_GameManager;
        private readonly ICurrentUser m_IdentityProvider;

        public ClockController() : this(null,null)
        { }

        public ClockController(IGameManager manager = null, ICurrentUser identityProvider = null)
        {
            m_GameManager = manager ?? new GameManager();
            m_IdentityProvider = identityProvider ?? new CurrentUserProvider();
        }

        [System.Web.Mvc.HttpPost]
        [VerifyIsParticipant]
        public ActionResult PlayerReady(int id)
        {
            var game = m_GameManager.FetchGame(id);
            var clock = m_GameManager.Clock(id);

            if (game == null || clock == null)
                return Json(new {status = "NULL"});

            var playerColor = game.CurrentPlayerColor(m_IdentityProvider.CurrentUser);
            var status = m_GameManager.PlayerReady(id, playerColor);
            var jsonStatus = (status == PlayerReadyStatus.Both ? "OK" : "WAIT");

            IHubContext hubContext = GlobalHost.ConnectionManager.GetHubContext<UpdateServer>();
            hubContext.Clients.Group(game.GameId.ToString()).startClock(new {status = jsonStatus, who = playerColor});

            return Json(new {status = jsonStatus });
        }

        [System.Web.Mvc.HttpPost]
        public ActionResult RefreshClock(int id)
        {
            var clock = m_GameManager.Clock(id);

            if (clock == null)
                return Json(new {status = "NULL"});

            int timeLeftBlack = clock.TimeLimitMs - clock.TimeElapsedBlackMs;
            int timeLeftWhite = clock.TimeLimitMs - clock.TimeElapsedWhiteMs;

            return Json(new {timeleftblack = timeLeftBlack, timeleftwhite = timeLeftWhite, status = "OK"});
        }
    }
}