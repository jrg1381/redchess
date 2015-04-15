﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Http;
using Antlr.Runtime;
using Chess.Models;
using Chess.Repositories;
using Microsoft.AspNet.SignalR;
using RedChess.ChessCommon.Enumerations;
using RedChess.EngineFactory;
/*
namespace Chess.Controllers
{
    public class BoardCreationResult
    {
        public string Status { get; set; }
        public string Message { get; set; }
        public int Id { get; set; }
    }

    public class PlayMoveResult
    {
        public string Status { get; set; }
        public string Fen { get; set; }
        public string Message { get; set; }
    }

    public class ChessApiController : ApiController
    {
        private readonly ChessContext m_dbChessContext = new ChessContext();

        [HttpGet]
        public BoardCreationResult Create(int playerId, int opponentId)
        {
            if (!VerifyUser(playerId)) return new BoardCreationResult { Status = "FAIL", Message = "playerId invalid" };
            if (!VerifyUser(opponentId)) return new BoardCreationResult { Status = "FAIL", Message = "opponentId invalid" };

            var board = BoardFactory.CreateInstance();
            //var dto = (new GameRepository()).Add(board, playerId, opponentId,);
            //dto.UpdateMessage(dto.Id);

            return new BoardCreationResult {Status = "OK", Message = "Board created", Id = 0};
        }

        [HttpGet]
        public Dictionary<int,string> Accounts()
        {
            return m_dbChessContext.UserProfiles.ToDictionary(user => user.UserId, user => user.UserName);
        }

        [HttpGet]
        public Dictionary<int, GameDto> Boards()
        {
            return m_dbChessContext.Boards.ToDictionary(b => b.GameId, b => b);
        }

        [HttpGet]
        public UserProfile Account(int id)
        {
            return m_dbChessContext.UserProfiles.Find(id);
        }

        [HttpGet]
        public GameDto Board(int id)
        {
            return m_dbChessContext.Boards.Find(id);
        }

        [HttpGet]
        public PlayMoveResult PlayMove(int id, string start, string end)
        {
            var board = (new GameRepository()).FindById(id);

            if (board.GameOver)
            {
                return new PlayMoveResult { Status = "FAIL", Fen = board.Fen, Message = "Game is over" };
            }

            if (!board.Move((Location)Enum.Parse(typeof(Location), start), (Location)Enum.Parse(typeof(Location), end)))
            {
                return new PlayMoveResult { Status = "FAIL", Fen = board.Fen, Message = "Invalid move" };
            }

            try
            {
                board.PromotePiece("Q");
            }
            catch(Exception)
            { }

            int nextMoveNumber = m_dbChessContext.HistoryEntries.Where(x => x.GameId == id).Max(x => x.MoveNumber) + 1;
            m_dbChessContext.HistoryEntries.Add(new HistoryEntry() { Fen = board.Fen, GameId = id, MoveNumber = nextMoveNumber, Move = board.LastMove });
            //board.UpdateMessage();
            m_dbChessContext.SaveChanges();

            var jsonObject = new { fen = board.Fen, message = board.Status + " [API updated board]", movefrom = start, moveto = end, status = "OK", mayClaimDraw = board.MayClaimDraw };
            IHubContext hubContext = GlobalHost.ConnectionManager.GetHubContext<UpdateServer>();
            hubContext.Clients.Group(id.ToString()).addMessage(jsonObject);

            return new PlayMoveResult { Status = "OK", Fen = board.Fen, Message = board.Status };
        }

        private bool VerifyUser(int playerId)
        {
            return m_dbChessContext.UserProfiles.Any(x => x.UserId == playerId);
        }
    }
}*/
