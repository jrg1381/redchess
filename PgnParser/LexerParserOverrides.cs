using System;
using System.Collections.Generic;
using Antlr4.Runtime;
using Antlr4.Runtime.Atn;

namespace Redchess.Pgn
{
    public partial class PgnParser
    {
        public bool PlayGame;
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
