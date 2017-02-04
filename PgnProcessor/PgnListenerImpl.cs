using System.Collections.Generic;
using Redchess.Pgn;

namespace RedChess.PgnProcessor
{
    internal class PgnListenerImpl : PgnBaseListener
    {
        private static readonly char[] DoubleQuote = {'"'};
        private static readonly char[] EqualsSign = { '=' };
        private readonly Dictionary<string, string> m_Tags;
        private readonly IPgnProcessor m_Processor;
        private int m_VariantDepth;
        private readonly bool m_PlayGame;

        private static string TrimQuotes(string s)
        {
            return s.Trim(DoubleQuote).Replace("\"\"", "\"");

        }

        internal PgnListenerImpl(Dictionary<string, string> tags, IPgnProcessor processor, bool playGame)
        {
            m_Tags = tags;
            m_Processor = processor;
            m_PlayGame = playGame;
        }

        public override void EnterBlackTag(PgnParser.BlackTagContext context)
        {
            m_Tags["Black"] = TrimQuotes(context.tag.Text);
        }

        public override void EnterWhiteTag(PgnParser.WhiteTagContext context)
        {
            m_Tags["White"] = TrimQuotes(context.tag.Text);
        }

        public override void EnterResultTag(PgnParser.ResultTagContext context)
        {
            m_Tags["Result"] = TrimQuotes(context.tag.Text);
        }

        public override void EnterEventTag(PgnParser.EventTagContext context)
        {
            m_Tags["Event"] = TrimQuotes(context.tag.Text);
        }

        public override void EnterOptionalTag(PgnParser.OptionalTagContext context)
        {
            var trimmedText = TrimQuotes(context.tagValue.Text);
            m_Tags[context.tag.Text] = trimmedText;

            if (context.tag.Text == "FEN")
            {
                // rnbqkbnr/pppppppp/8/8/4P3/8/PPPP1PPP/RNBQKBNR b KQkq e3 0 1
                m_Processor.DoFen(trimmedText);
            }
        }

        public override void EnterGame(PgnParser.GameContext context)
        {
            m_Tags.Clear();
            m_Processor.ResetGame();
        }

        public override void EnterEnterVariant(PgnParser.EnterVariantContext context)
        {
            m_VariantDepth++;
        }

        public override void EnterLeaveVariant(PgnParser.LeaveVariantContext context)
        {
            m_VariantDepth--;
        }

        public override void EnterIndividualMove(PgnParser.IndividualMoveContext context)
        {
            if (m_PlayGame && m_VariantDepth == 0)
            {
                m_Processor.ProcessMove(context.foo,
                    context.promote == null ? "" : context.promote.Text.TrimStart(EqualsSign),
                    context.checkormate == null ? "" : context.checkormate.Text,
                    context.annotation_glyph == null ? "" : context.annotation_glyph.Text);
            }
        }
    }
}