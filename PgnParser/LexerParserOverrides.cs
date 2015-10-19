using System;
using System.IO;
using System.Text;
using Antlr4.Runtime;
using Antlr4.Runtime.Atn;
using Redchess.Pgn;

namespace Redchess.Pgn
{
    public interface IPgnProcessor
    {
        void ResetGame();
        void ProcessMove(IToken token, string promotedPiece, string checkOrMate, string annotationGlyph);
        void DoFen(string fen);
    }

    public class ParserErrorListener : IAntlrErrorListener<IToken>
    {
        private readonly Action<string> m_onErrorAction;

        public ParserErrorListener(Action<string> onErrorAction)
        {
            m_onErrorAction = onErrorAction;
        }

        public void SyntaxError(IRecognizer recognizer, IToken offendingSymbol, int line, int charPositionInLine, string msg, RecognitionException e)
        {
            m_onErrorAction("Error in parser at line " + line + ":" + charPositionInLine + "[" + offendingSymbol.Text + "]");
        }
    }

    public class LexerErrorListener : IAntlrErrorListener<int>
    {
        private readonly Action<string> m_onErrorAction;

        public LexerErrorListener(Action<string> onErrorAction)
        {
            m_onErrorAction = onErrorAction;
        }

        public void SyntaxError(IRecognizer recognizer, int offendingSymbol, int line, int charPositionInLine, string msg, RecognitionException e)
        {
            m_onErrorAction("Error in parser at line " + line + ":" + charPositionInLine + "[" + offendingSymbol + "]");
        }
    }

    public partial class PgnParser
    {
        private readonly IPgnProcessor m_processor;

        public PgnParser(ITokenStream tokenStream, IPgnProcessor processor, Action<string> onErrorAction)
            : base(tokenStream)
        {
            m_processor = processor;
            // https://groups.google.com/forum/#!topic/antlr-discussion/8T6qaANpi94
            base.Interpreter = new ParserATNSimulator(this, _ATN);
            base.AddErrorListener(new ParserErrorListener(onErrorAction));
        }
    }

    partial class PgnLexer
    {
        public PgnLexer(ICharStream stream, Action<string> onErrorAction)
            : base(stream)
        {
            // https://groups.google.com/forum/#!topic/antlr-discussion/8T6qaANpi94
            base.Interpreter = new LexerATNSimulator(this, _ATN);
            base.AddErrorListener(new LexerErrorListener(onErrorAction));
        }
    }
}
