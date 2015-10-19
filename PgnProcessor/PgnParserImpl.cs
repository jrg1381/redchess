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
        private static readonly char[] s_doubleQuote = {'"'};

        private readonly Dictionary<string, string> m_tags;
        private readonly IPgnProcessor m_processor;
        private int m_variantDepth = 0;
        private bool m_playGame;

        private static string TrimQuotes(string s)
        {
            return s.Trim(s_doubleQuote).Replace("\"\"", "\"");

        }

        public PgnListenerImpl(Dictionary<string, string> tags, IPgnProcessor processor, bool playGame)
        {
            m_tags = tags;
            m_processor = processor;
            m_playGame = playGame;
        }

        public override void EnterBlackTag(PgnParser.BlackTagContext context)
        {
            m_tags["Black"] = TrimQuotes(context.tag.Text);
        }

        public override void EnterWhiteTag(PgnParser.WhiteTagContext context)
        {
            m_tags["White"] = TrimQuotes(context.tag.Text);
        }

        public override void EnterResultTag(PgnParser.ResultTagContext context)
        {
            m_tags["Result"] = TrimQuotes(context.tag.Text);
        }

        public override void EnterEventTag(PgnParser.EventTagContext context)
        {
            m_tags["Event"] = TrimQuotes(context.tag.Text);
        }

        public override void EnterOptionalTag(PgnParser.OptionalTagContext context)
        {
            var trimmedText = context.tagValue.Text;
            m_tags[context.tag.Text] = TrimQuotes(context.tagValue.Text);

            if (context.tagValue.Text == "FEN")
            {
                // rnbqkbnr/pppppppp/8/8/4P3/8/PPPP1PPP/RNBQKBNR b KQkq e3 0 1
                m_processor.DoFen(trimmedText);
            }
        }

        public override void EnterGame(PgnParser.GameContext context)
        {
            m_tags.Clear();
            m_processor.ResetGame();
        }

        public override void EnterEnterVariant(PgnParser.EnterVariantContext context)
        {
            m_variantDepth++;
        }

        public override void EnterLeaveVariant(PgnParser.LeaveVariantContext context)
        {
            m_variantDepth--;
        }

        public override void EnterIndividualMove(PgnParser.IndividualMoveContext context)
        {
            if (m_playGame && m_variantDepth == 0)
            {
                m_processor.ProcessMove(context.foo,
                    context.promote == null ? "" : context.promote.Text.TrimStart(new[] {'='}),
                    context.checkormate == null ? "" : context.checkormate.Text,
                    context.annotation_glyph == null ? "" : context.annotation_glyph.Text);
            }
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
            var listener = new PgnListenerImpl(m_tags, processor, playGame);
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