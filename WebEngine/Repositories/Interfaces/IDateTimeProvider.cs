using System;

namespace RedChess.WebEngine.Repositories.Interfaces
{
    public interface IDateTimeProvider
    {
        DateTime UtcNow { get; }
    }
}