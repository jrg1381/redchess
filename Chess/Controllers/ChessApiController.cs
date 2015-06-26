using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using System.Web.Http;
using System.Web.Mvc;
using Microsoft.Ajax.Utilities;
using RedChess.WebEngine.Repositories;

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
        public object Board(int id)
        {
            return Json(GameBindingToGameData(m_gameManager.FetchGame(id)));
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
