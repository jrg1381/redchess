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

namespace Chess.Controllers
{
    public class HistoryController : Controller
    {
        readonly IGameManager m_gameManager = new GameManager();
        private readonly ICurrentUser m_identityProvider = new CurrentUserImpl();

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

        public ActionResult ShowMove(string gameId, string moveNumber)
		{
			int game = Int32.Parse(gameId);
			int move = Int32.Parse(moveNumber);

            var entries = m_gameManager.FindAllMoves(game);

            if (!entries.Any())
            {
                return HttpNotFound("No such game or move");
            }

            ViewBag.MoveNumber = move;
            return View("History", entries);
		}

        [HttpGet]
        public ContentResult Pgn(int id)
        {
            var content = new ContentResult {ContentType = @"text\plain"};

            var entries = m_gameManager.FindAllMoves(id).ToList();

            if (!entries.Any())
            {
                content.Content = "NOT FOUND";
                return content;
            }
            
           var gameDetails = m_gameManager.FetchGame(id);

            content.Content = GeneratePgn(entries, gameDetails);
            return content;
        }

        internal string GeneratePgn(IList<HistoryEntry> entries, IGameBinding gameDetails)
        {
            var numberOfMoves = entries.Count;
            var result = "*";

            if (gameDetails.GameOver)
            {
                if (gameDetails.UserProfileWinner == null)
                {
                    result = "1/2-1/2";
                }
                else
                {
                    if (gameDetails.UserProfileWhite.UserId == gameDetails.UserProfileWinner.UserId)
                    {
                        result = "1-0";
                    }

                    if (gameDetails.UserProfileBlack.UserId == gameDetails.UserProfileWinner.UserId)
                    {
                        result = "0-1";
                    }
                }
            }

            var pgnBuilder = new StringBuilder();

            pgnBuilder.AppendLine("[Event \"Casual Game\"]")
            .AppendLine("[Site \"?\"]")
            .AppendLine("[Round \"?\"]")
            .AppendFormat("[Date \"{0:yyyy.MM.dd}\"]\r\n", gameDetails.CreationDate)
            .AppendFormat("[White \"{0}\"]\r\n", gameDetails.UserProfileWhite.UserName)
            .AppendFormat("[Black \"{0}\"]\r\n", gameDetails.UserProfileBlack.UserName)
            .AppendFormat("[Result \"{0}\"]\r\n", result);

            if (gameDetails.Clock != null)
            {
                pgnBuilder.AppendFormat("[TimeControl \"{0}\"]\r\n", gameDetails.Clock.TimeLimitMs / 1000);
            }

            pgnBuilder.AppendLine();

            var moveNumber = 1;
            for (var i = 1; i < numberOfMoves; i += 2)
            {
                var nextMoveIndex = i + 1;
                pgnBuilder.AppendFormat("{0}. {1}", moveNumber++, entries[i].Move);
                if (nextMoveIndex < numberOfMoves)
                {
                    pgnBuilder.AppendFormat(" {0} ", entries[nextMoveIndex].Move);
                }
            }

            pgnBuilder.AppendFormat(" {0}", result);
            return pgnBuilder.ToString();
        }
    }
}
