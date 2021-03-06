﻿using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using System.Web;
using System.Web.Http;
using System.Web.Http.Results;
using RedChess.WebEngine.Models;
using RedChess.WebEngine.Repositories;
using RedChess.WebEngine.Repositories.Interfaces;

namespace Chess.Controllers
{
    public class ChessApiController : ApiController
    {
        private readonly IGameManager m_GameManager;
        private readonly ICurrentUser m_IdentityProvider;

        class DateElo
        {
            public DateTime Date;
            public int Elo;
        }

        // ReSharper disable once RedundantArgumentDefaultValue
        public ChessApiController() : this(null, null)
        {
        }

        public ChessApiController(IGameManager manager = null, ICurrentUser identityProvider = null)
        {
            m_GameManager = manager ?? new GameManager();
            m_IdentityProvider = identityProvider ?? new CurrentUserProvider();
        }

        [HttpGet]
        public object Boards()
        {
            var queryString = HttpContext.Current.Server.UrlDecode(HttpContext.Current.Request.QueryString.ToString());
            return m_GameManager.FindWhere(queryString);
        }

        [HttpGet]
        public object Avatar()
        {
            var emailHash = m_GameManager.GetEmailHashForUsername(HttpContext.Current.User.Identity.Name);
            if (String.IsNullOrEmpty(emailHash))
                return "";
            return Json($"https://www.gravatar.com/avatar/{emailHash}?s=32&d=mm");
        }

        [HttpGet]
        public object Stats()
        {
            return Json(m_GameManager.Stats());
        }

        [HttpGet]
        public object Moves(int id)
        {
            try
            {
                dynamic data = new ExpandoObject();
                var game = m_GameManager.FetchGame(id);
                var allMoves = m_GameManager.FindAllMoves(id).ToList();

                data.Moves = allMoves.Select<HistoryEntry, object>(m => new {m.Fen, m.Move});
                data.Description = game.Description;
                data.IsParticipant = game.UserProfileBlack.UserId == m_IdentityProvider.CurrentUserId ||
                                     game.UserProfileWhite.UserId == m_IdentityProvider.CurrentUserId;
                data.Analysis = m_GameManager.AnalysisForGameMoves(id);
                data.GameOver = game.GameOver;
                data.Status = game.Status;
                data.Winner = "";

                if (game.UserProfileWinner != null)
                {
                    if (game.UserProfileWinner.UserId == game.UserProfileWhite.UserId)
                        data.Winner = "w";
                    else if (game.UserProfileWinner.UserId == game.UserProfileBlack.UserId)
                        data.Winner = "b";
                }

                return Json(data);
            }
            catch (Exception)
            {
                return new StatusCodeResult(HttpStatusCode.NotFound, this);
            }
        }

        [HttpGet]
        public object Elo()
        {
            var profilesTask = Task.Factory.StartNew(() => m_GameManager.AllUserProfiles());
            var eloTableTask = Task.Factory.StartNew(() => m_GameManager.EloTable());
            var winlossTask = Task.Factory.StartNew(() => m_GameManager.Stats());
            var lastUpdatedTask = Task.Factory.StartNew(() => m_GameManager.LastEloUpdate());

            Task.WaitAll(profilesTask, eloTableTask, winlossTask);

            var profiles = profilesTask.Result;
            var eloTable = eloTableTask.Result;
            var winlossStats = winlossTask.Result;
            var lastUpdated = lastUpdatedTask.Result;

            var eloData = new Dictionary<int, List<DateElo>>();

            foreach (var d in eloTable)
            {
                if (!eloData.ContainsKey(d.UserId))
                {
                    eloData[d.UserId] = new List<DateElo>();
                }
                
                eloData[d.UserId].Add(new DateElo { Date = d.Date, Elo = d.Elo });
            }

            foreach (var key in eloData.Keys)
            {
                eloData[key].Sort((a, b) => a.Date.CompareTo(b.Date));
            }

            dynamic response = new ExpandoObject();
            response.EloData = eloData;
            response.Profiles = profiles;
            response.WinLoss = winlossStats;
            response.LastUpdated = lastUpdated;

            return Json(response);
        }
    }
}
