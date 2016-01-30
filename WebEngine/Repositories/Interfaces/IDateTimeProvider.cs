using System;

namespace RedChess.WebEngine.Repositories.Interfaces
{
    internal interface IDateTimeProvider
    {
        DateTime UtcNow { get; }
    }
}