﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web.Mvc;
using Chess.Models;
using RedChess.WebEngine.Models;
using RedChess.WebEngine.Repositories;
using RedChess.EngineFactory;
using RedChess.WebEngine.Repositories.Interfaces;

namespace Chess.Controllers
{
    public class HistoryController : Controller
    {
        readonly IGameManager m_gameManager;
        private readonly ICurrentUser m_identityProvider;

        public HistoryController(): this(null, null)
        { }

        public HistoryController(IGameManager gameManager = null, ICurrentUser identityProvider = null)
        {
            m_gameManager = gameManager ?? new GameManager();
            m_identityProvider = identityProvider ?? new CurrentUserProvider();
        }

        public ActionResult Index()
        {
			return HttpNotFound();
        }

        public ActionResult PlayFromHere(string move, string gameId)
        {
            bool playAsBlack = false;
            int moveNumber = Int32.Parse(move);
            int game = Int32.Parse(gameId);
            int opponent;

            var thisGame = m_gameManager.FetchGame(game);

            if (thisGame == null)
            {
                return View("Error", new HandleErrorInfo(new ArgumentException("Source game not found in database"), "History", "PlayFromHere"));
            }

            if (thisGame.UserProfileBlack.UserName == m_identityProvider.CurrentUser)
            {
                opponent = thisGame.UserProfileWhite.UserId;
                playAsBlack = true;
            }
            else
            {
                opponent = thisGame.UserProfileBlack.UserId;
            }

            var historyEntry = m_gameManager.FindByGameIdAndMoveNumber(game, moveNumber);
            if (historyEntry == null)
            {
                return View("Error", new HandleErrorInfo(new ArgumentException("Source game not found in history"), "History", "PlayFromHere"));
            }

            var newBoard = BoardFactory.CreateInstance();
            newBoard.FromFen(historyEntry.Fen);

            int gameIdOfNewGame = m_gameManager.CloneBoard(newBoard, opponent, m_identityProvider.CurrentUser, playAsBlack, game, historyEntry.MoveNumber);

            return RedirectToAction("Details", "Board", new { id = gameIdOfNewGame});
        }

        public ActionResult ShowMove(int id)
		{
            var entries = m_gameManager.FindAllMoves(id);

            if (!entries.Any())
            {
                return HttpNotFound("No such game or move");
            }

            return View("History", id);
		}

        [HttpGet]
        public ContentResult Pgn(int id)
        {
            var content = new ContentResult
            {
                ContentType = @"text\plain",
                Content = m_gameManager.PgnText(id)
            };

            return content;
        }
    }
}
