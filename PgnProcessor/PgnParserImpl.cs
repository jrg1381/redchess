using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Mime;
using Antlr4.Runtime;
using Antlr4.Runtime.Tree;
using Redchess.Pgn;
using RedChess.ChessCommon.Interfaces;
using RedChess.ChessCommon;
using RedChess.ChessCommon.Enumerations;

namespace RedChess.PgnProcessor
{
    public class PgnParserImpl : IParse
    {
        private readonly Dictionary<string, string> m_tags;

        public PgnParserImpl()
        {
            m_tags = new Dictionary<string, string>();
        }

        public void Parse(string text, Action<string, string, ChessMove> onMoveAction, Action<string> onErrorAction, Action onGameOverAction, bool playGame = true)
        {
            m_tags.Clear(); // In case method is called multiple times
            var processor = new PgnProcessor(onMoveAction, onGameOverAction);
            var lexer = new PgnLexer(new AntlrInputStream(text), onErrorAction);
            var tokenStream = new CommonTokenStream(lexer);
            var parser = new PgnParser(tokenStream, onErrorAction);
            var tree = parser.parse();
            var walker = new ParseTreeWalker();
            var listener = new PgnListenerImpl(m_tags, processor, playGame);
            // Will populate m_tags and validate the moves by playing them
            walker.Walk(listener, tree);
        }

        public string Tag(string key)
        {
            return m_tags[key];
        }

        public IDictionary<string, string> Tags
        {
            get { return m_tags; }
        }
    }
}