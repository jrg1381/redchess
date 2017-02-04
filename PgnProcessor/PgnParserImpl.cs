using System;
using System.Collections.Generic;
using Antlr4.Runtime;
using Antlr4.Runtime.Tree;
using Redchess.Pgn;
using RedChess.ChessCommon.Interfaces;
using RedChess.ChessCommon;

namespace RedChess.PgnProcessor
{
    public class PgnParserImpl : IParse
    {
        private readonly Dictionary<string, string> m_Tags;

        public PgnParserImpl()
        {
            m_Tags = new Dictionary<string, string>();
        }

        public void Parse(string text, Action<string, string, ChessMove> onMoveAction, Action<string> onErrorAction, Action onGameOverAction, bool playGame = true)
        {
            m_Tags.Clear(); // In case method is called multiple times
            var processor = new PgnProcessor(onMoveAction, onGameOverAction);
            var lexer = new PgnLexer(new AntlrInputStream(text), onErrorAction);
            var tokenStream = new CommonTokenStream(lexer);
            var parser = new PgnParser(tokenStream, onErrorAction);
            var tree = parser.parse();
            var walker = new ParseTreeWalker();
            var listener = new PgnListenerImpl(m_Tags, processor, playGame);
            // Will populate m_tags and validate the moves by playing them
            walker.Walk(listener, tree);
        }

        public string Tag(string key)
        {
            return m_Tags[key];
        }

        public IDictionary<string, string> Tags => m_Tags;
    }
}