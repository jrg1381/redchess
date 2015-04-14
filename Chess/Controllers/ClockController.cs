using System;
using System.Linq;
using System.Web.Mvc;
using Chess.Models;
using Chess.Repositories;
using Microsoft.AspNet.SignalR;

namespace Chess.Controllers
{
    public class ClockController : Controller
    {
        private readonly GameRepository m_repository = new GameRepository();
        private readonly ClockRepository m_clockRepository = new ClockRepository();
        private readonly ICurrentUser m_identityProvider = new CurrentUserImpl();

        [System.Web.Mvc.HttpPost]
        public ActionResult PlayerReady(int id)
        {
            var game = m_repository.FindById(id);
            var clock = m_clockRepository.Clock(id);

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

            string status = "WAIT";

            if (clock.PlayersReady == 3)
            {
                clock.LastActionWhite = DateTime.UtcNow;
                status = "OK";
            }

            m_clockRepository.SaveClock(clock);

            IHubContext hubContext = GlobalHost.ConnectionManager.GetHubContext<UpdateServer>();
            hubContext.Clients.Group(game.Id.ToString()).startClock(new {status = status, who = playerColor});
            return Json(new {status = status});
        }

        [System.Web.Mvc.HttpPost]
        public ActionResult RefreshClock(int id)
        {
            var clock = m_clockRepository.Clock(id);

            if (clock == null)
                return Json(new {status = "NULL"});

            int timeLeftBlack = clock.TimeLimitMs - clock.TimeElapsedBlackMs;
            int timeLeftWhite = clock.TimeLimitMs - clock.TimeElapsedWhiteMs;

            return Json(new {timeleftblack = timeLeftBlack, timeleftwhite = timeLeftWhite, status = "OK"});
        }
    }
}