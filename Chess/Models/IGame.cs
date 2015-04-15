using System;
using System.Web.Mvc;
using RedChess.ChessCommon.Enumerations;

namespace Chess.Models
{
    public interface IGame
    {
        void EndGameWithMessage(int gameId, string message);
        bool Move(int gameId, Location start, Location end);
        void PromotePiece(int gameId, string typeToPromoteTo);
        string Description(int gameId);
        bool ShouldLockUi(int gameId);
        IClock Clock(int gameId);
        string CurrentPlayerColor(int gameId, string userName);
    }
}