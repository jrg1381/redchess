using System;
using Antlr4.Runtime;

namespace Redchess.Pgn
{
    internal class LexerErrorListener : IAntlrErrorListener<int>
    {
        private readonly Action<string> m_OnErrorAction;

        public LexerErrorListener(Action<string> onErrorAction)
        {
            m_OnErrorAction = onErrorAction;
        }

        public void SyntaxError(IRecognizer recognizer, int offendingSymbol, int line, int charPositionInLine, string msg, RecognitionException e)
        {
            m_OnErrorAction(msg);
        }
    }
}