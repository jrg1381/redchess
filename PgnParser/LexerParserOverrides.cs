using System;
using Antlr4.Runtime;
using Antlr4.Runtime.Atn;

namespace Redchess.Pgn
{
    public sealed partial class PgnParser
    {
        public PgnParser(ITokenStream tokenStream, Action<string> onErrorAction)
            : base(tokenStream)
        {
            // https://groups.google.com/forum/#!topic/antlr-discussion/8T6qaANpi94
            Interpreter = new ParserATNSimulator(this, _ATN);
            AddErrorListener(new ParserErrorListener(onErrorAction));
        }
    }

    partial class PgnLexer
    {
        public PgnLexer(ICharStream stream, Action<string> onErrorAction)
            : base(stream)
        {
            // https://groups.google.com/forum/#!topic/antlr-discussion/8T6qaANpi94
            Interpreter = new LexerATNSimulator(this, _ATN);
            AddErrorListener(new LexerErrorListener(onErrorAction));
        }
    }
}
