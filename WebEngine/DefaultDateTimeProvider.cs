using System;
using RedChess.WebEngine.Repositories.Interfaces;

namespace RedChess.WebEngine
{
    public class DefaultDateTimeProvider : IDateTimeProvider
    {
        public DateTime UtcNow => DateTime.UtcNow;
    }
}
