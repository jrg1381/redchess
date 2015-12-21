using System;
using System.Web.Mvc;
using RedChess.WebEngine.Repositories;
using Microsoft.AspNet.SignalR;
using RedChess.ChessCommon;
using RedChess.WebEngine.Repositories.Interfaces;

namespace Chess.Controllers
{
    public class ClockController : Controller
    {
        private readonly IGameManager m_gameManager;
        private readonly ICurrentUser m_identityProvider;

        public ClockController() : this(null,null)
        { }

        public ClockController(IGameManager manager = null, ICurrentUser identityProvider = null)
        {
            m_gameManager = manager ?? new GameManager();
            m_identityProvider = identityProvider ?? new CurrentUserImpl();
        }

        [System.Web.Mvc.HttpPost]
        public ActionResult PlayerReady(int id)
        {
            string status = "WAIT";

            /* 
            There is a race condition here when both players hit their clocks at the same time.
            Sometimes the 2nd player's write overwrites the first player's without the 2nd player
            having seen that the first player is ready. This can be fixed by locking on the game id,
            but it's no good to lock on value types (two instances of "10" are different objects)
            so we use a LockFactory which gives an object suitable for locking on.
            */

            lock (LockFactory.GetLock(id))
            {
                var game = m_gameManager.FetchGame(id);
                var clock = m_gameManager.Clock(id);

                if (game == null || clock == null)
                    return Json(new {status = "NULL"});

                string playerColor = game.CurrentPlayerColor(m_identityProvider.CurrentUser);
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

                if (clock.PlayersReady == 3)
                {
                    clock.LastActionWhite = DateTime.UtcNow;
                    status = "OK";
                }

                m_gameManager.SaveClock(clock);

                IHubContext hubContext = GlobalHost.ConnectionManager.GetHubContext<UpdateServer>();
                hubContext.Clients.Group(game.GameId.ToString()).startClock(new {status = status, who = playerColor});
            }

            if(status == "OK")
                LockFactory.Purge(id);

            return Json(new { status = status });
        }

        [System.Web.Mvc.HttpPost]
        public ActionResult RefreshClock(int id)
        {
            var clock = m_gameManager.Clock(id);

            if (clock == null)
                return Json(new {status = "NULL"});

            int timeLeftBlack = clock.TimeLimitMs - clock.TimeElapsedBlackMs;
            int timeLeftWhite = clock.TimeLimitMs - clock.TimeElapsedWhiteMs;

            return Json(new {timeleftblack = timeLeftBlack, timeleftwhite = timeLeftWhite, status = "OK"});
        }
    }
}