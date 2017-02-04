using System;
using Antlr4.Runtime;

namespace Redchess.Pgn
{
    internal class ParserErrorListener : IAntlrErrorListener<IToken>
    {
        private readonly Action<string> m_OnErrorAction;

        public ParserErrorListener(Action<string> onErrorAction)
        {
            m_OnErrorAction = onErrorAction;
        }

        public void SyntaxError(IRecognizer recognizer, IToken offendingSymbol, int line, int charPositionInLine, string msg, RecognitionException e)
        {
            m_OnErrorAction("Error in parser at line " + line + ":" + charPositionInLine + "[" + offendingSymbol.Text + "]");
        }
    }
}