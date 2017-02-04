using System;
using System.Collections.Generic;

namespace RedChess.ChessCommon.Interfaces
{
    public interface IParse
    {
        void Parse(string text, Action<string, string, ChessMove> onMoveAction, Action<string> onErrorAction, Action onGameOverAction, bool playGame = true);
        string Tag(string key);
        IDictionary<string, string> Tags { get; }
    }
}