using System;

namespace RedChess.WebEngine.Repositories.Interfaces
{
    public interface IEloHistory
    {
        DateTime Date { get; }
        int UserId { get; }
        int Elo { get; }
    }
}