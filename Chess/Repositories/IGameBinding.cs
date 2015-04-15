using System;
using Chess.Models;

namespace Chess.Repositories
{
    public interface IGameBinding
    {
        string Description { get; }
        bool ShouldLockUi { get; }
        string Status { get; }
        int GameId { get; }
        string Fen { get; }
        bool GameOver { get; }
        DateTime CreationDate { get; }
        DateTime CompletionDate { get; }
        UserProfile UserProfileBlack { get; }
        UserProfile UserProfileWhite { get; }
        bool MayClaimDraw { get; }
        string LastMove { get; }
        IClock Clock { get; }
        string CurrentPlayerColor(string userName);
    }
}