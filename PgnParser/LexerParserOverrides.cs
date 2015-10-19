using System;
using System.Collections.Generic;
using Antlr4.Runtime;
using Antlr4.Runtime.Atn;

namespace Redchess.Pgn
{
    public interface IPgnProcessor
    {
        void ResetGame();
        void ProcessMove(IToken token, string promotedPiece, string checkOrMate, string annotationGlyph);
        void DoFen(string fen);
    }

    public partial class PgnParser
    {
        private int m_variantDepth;
        public bool PlayGame;
        private static readonly char[] c_doubleQuote = { '"' };

        private readonly Dictionary<string, string> m_optionalTags = new Dictionary<string, string>();
        public IDictionary<string, string> OptionalTags { get { return m_optionalTags; } }

        public string Event { get; private set; }
        public string Site { get; private set; }
        public string Date { get; private set; }
        public string Round { get; private set; }
        public string Black { get; private set; }
        public string White { get; private set; }
        public string Result { get; private set; }

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
