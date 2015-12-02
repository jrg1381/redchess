using System;

namespace RedChess.WebEngine.Models
{
    public interface ILogEntry
    {
        DateTime PreciseTimeStamp { get; set; }
        string Message { get; set; }
        int Pid { get; set; }
        int Tid { get; set; }
    }
}