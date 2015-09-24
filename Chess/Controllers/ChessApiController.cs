using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Http;
using System.Web.Http.Results;
using System.Web.Mvc;
using Microsoft.Ajax.Utilities;
using RedChess.WebEngine.Models;
using RedChess.WebEngine.Repositories;
using RedChess.WebEngine.Repositories.Interfaces;

namespace Chess.Controllers
{
    public class ChessApiController : ApiController
    {
        private readonly IGameManager m_gameManager;

        public ChessApiController() : this(null)
        {
        }

        public ChessApiController(IGameManager manager = null)
        {
            m_gameManager = manager ?? new GameManager();
        }

        [System.Web.Http.HttpGet]
        public object Boards()
        {
            return Json(m_gameManager.FindAll().ToDictionary(game => game.GameId, GameBindingToGameData));
        }

        [System.Web.Http.HttpGet]
        public object Boards(string id)
        {
            return Json(m_gameManager.WithPlayer(id).ToDictionary(game => game.GameId, GameBindingToGameData));
        }

        [System.Web.Http.HttpGet]
        public object Moves(int id)
        {
            try
            {
                dynamic data = new ExpandoObject();
                var game = m_gameManager.FetchGame(id);
                var allMoves = m_gameManager.FindAllMoves(id).ToList();

                data.Moves = allMoves.Select<HistoryEntry, object>(m => new { m.Fen, m.Move });
                data.Description = game.Description;
                data.IsParticipant = m_gameManager.IsParticipant(HttpContext.Current.User.Identity.Name, id);

                return Json(data);
            }
            catch (Exception)
            {
                return new StatusCodeResult(HttpStatusCode.NotFound, this);
            }
        }

        [System.Web.Http.HttpGet]
        public object Board(int id)
        {
            try
            {
                return Json(GameBindingToGameData(m_gameManager.FetchGame(id)));
            }
            catch (Exception)
            {
                return new StatusCodeResult(HttpStatusCode.NotFound, this);
            }
        }

        private static object GameBindingToGameData(IGameBinding game)
        {
            dynamic data = new ExpandoObject();

            data.Fen = game.Fen;
            data.Status = game.Status;
            data.CreationDate = game.CreationDate;
            data.Description = game.Description;
            data.White = game.UserProfileWhite.UserId;
            data.Black = game.UserProfileBlack.UserId;
            data.GameOver = game.GameOver;

            if (game.UserProfileWinner != null)
            {
                data.Winner = game.UserProfileWinner.UserId;
            }

            if (game.GameOver)
            {
                data.CompletionDate = game.CompletionDate;
            }

            return data;
        }
    }
}
