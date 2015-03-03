using System;
using System.Collections.Generic;
using RedChess.ChessCommon.Enumerations;

namespace RedChess.ChessCommon.Interfaces
{
    public interface IParse
    {
        void Parse(string text, Action<string, string, ChessMove> onMoveAction, Action<string> onErrorAction, Action onGameOverAction, bool playGame = true);
        string Event { get; set; }
        string Result { get; }
        IDictionary<string, string> Tags { get; }
    }
}