using System;
using System.Collections.Generic;

namespace RedChess.ChessCommon.Interfaces
{
    public interface IParse
    {
        void Parse(string text, Action<string, string> onMoveAction, Action<string> onErrorAction, bool playGame = true);
        string Event { get; set; }
        string Result { get; }
        IDictionary<string, string> Tags { get; }
    }
}