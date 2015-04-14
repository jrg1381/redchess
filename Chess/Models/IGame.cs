using System;
using RedChess.ChessCommon.Enumerations;

namespace Chess.Models
{
    public interface IGame
    {
        string Status { get; }
        int UserIdWhite { get; }
        int UserIdBlack { get; }
        bool MayClaimDraw { get; }
        bool GameOver { get; }
        DateTime CreationDate { get; }
        DateTime CompletionDate { get; }
        string LastMove { get; }
        string Fen { get; }
        UserProfile UserProfileWhite { get; }
        UserProfile UserProfileBlack { get; }
        string Turn { get; }
        bool IsUsersTurn(string userName);
        void UpdateMessage(int gameId);
        void EndGameWithMessage(int gameId, string message);
        bool Move(Location start, Location end);
        void PromotePiece(string typeToPromoteTo);
        int Id { get; }
        string Description { get; }
        Clock Clock { get; }

        string CurrentPlayerColor(string p);
    }
}