using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RedChess.WebEngine.Repositories.Interfaces;

namespace RedChess.WebEngine
{
    public class DefaultDateTimeProvider : IDateTimeProvider
    {
        public DateTime UtcNow => DateTime.UtcNow;
    }
}
