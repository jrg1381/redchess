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
    public class PgnListenerImpl : PgnBaseListener
    {
        private readonly Dictionary<string, string> m_tags;

        public PgnListenerImpl(Dictionary<string, string> tags)
        {
            m_tags = tags;
        }

        public override void EnterBlackTag(PgnParser.BlackTagContext context)
        {
            m_tags["Black"] = context.bar.Text;
        }

        public override void EnterWhiteTag(PgnParser.WhiteTagContext context)
        {
            m_tags["White"] = context.bar.Text;
        }

        public override void EnterResultTag(PgnParser.ResultTagContext context)
        {
            m_tags["Result"] = context.bar.Text;
        }

        public override void EnterEventTag(PgnParser.EventTagContext context)
        {
            m_tags["Event"] = context.bar.Text;
        }

        public override void EnterOptionalTag(PgnParser.OptionalTagContext context)
        {
            m_tags[context.bar.Text] = context.foo.Text;
        }
    }

    public class PgnParserImpl : IParse
    {
        private Dictionary<string, string> m_tags;

        public void Parse(string text, Action<string, string, ChessMove> onMoveAction, Action<string> onErrorAction, Action onGameOverAction, bool playGame = true)
        {
            m_tags = new Dictionary<string, string>();
            var processor = new PgnProcessor(onMoveAction, onGameOverAction);
            var lexer = new PgnLexer(new AntlrInputStream(text), onErrorAction);
            var tokenStream = new CommonTokenStream(lexer);
            var parser = new PgnParser(tokenStream, processor, onErrorAction)
            {
                PlayGame = playGame,
                BuildParseTree = true,
            };
            var tree = parser.parse();
            var walker = new ParseTreeWalker();
            var listener = new PgnListenerImpl(m_tags);
            walker.Walk(listener, tree);
        }

        public string Event
        {
            get { return m_tags["Event"]; }
        }

        public string Result
        {
            get { return m_tags["Result"]; }
        }

        public IDictionary<string, string> Tags
        {
            get { return m_tags; }
        }
    }
}