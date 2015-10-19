using System.Collections.Generic;
using Redchess.Pgn;

namespace RedChess.PgnProcessor
{
    internal class PgnListenerImpl : PgnBaseListener
    {
        private static readonly char[] s_doubleQuote = {'"'};
        private static readonly char[] s_equalsSign = { '=' };
        private readonly Dictionary<string, string> m_tags;
        private readonly IPgnProcessor m_processor;
        private int m_variantDepth;
        private readonly bool m_playGame;

        private static string TrimQuotes(string s)
        {
            return s.Trim(s_doubleQuote).Replace("\"\"", "\"");

        }

        internal PgnListenerImpl(Dictionary<string, string> tags, IPgnProcessor processor, bool playGame)
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
            var trimmedText = TrimQuotes(context.tagValue.Text);
            m_tags[context.tag.Text] = trimmedText;

            if (context.tag.Text == "FEN")
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
                    context.promote == null ? "" : context.promote.Text.TrimStart(s_equalsSign),
                    context.checkormate == null ? "" : context.checkormate.Text,
                    context.annotation_glyph == null ? "" : context.annotation_glyph.Text);
            }
        }
    }
}