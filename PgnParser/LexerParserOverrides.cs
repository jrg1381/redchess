using System;
using System.IO;
using Antlr.Runtime;

namespace Redchess.Pgn
{
    public interface IPgnProcessor
    {
        void ResetGame();
        void ProcessMove(IToken token, string promotedPiece, string checkOrMate, string annotationGlyph);
        void DoFen(string fen);
    }

    partial class PgnParser
    {
        private readonly IPgnProcessor m_processor;
        private readonly Action<string> m_onErrorAction;

        public PgnParser(ITokenStream tokenStream, IPgnProcessor processor, Action<string> onErrorAction)
            : base(tokenStream)
        {
            m_onErrorAction = onErrorAction;
            m_processor = processor;
            TraceDestination = Console.Out;
        }

        public override void ReportError(RecognitionException e)
        {
            base.ReportError(e);
            m_onErrorAction("Error in parser at line " + e.Line + ":" + e.CharPositionInLine + "[" + e.Token + "]");
        }
    }

    partial class PgnLexer
    {
        private readonly Action<string> m_onErrorAction;

        public PgnLexer(ICharStream stream, Action<string> onErrorAction) : base(stream)
        {
            m_onErrorAction = onErrorAction;
        }

        public override void ReportError(RecognitionException e)
        {
            base.ReportError(e);
            m_onErrorAction("Error in lexer at line " + e.Line + ":" + e.CharPositionInLine);
        }
    }
}
