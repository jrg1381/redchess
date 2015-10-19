using Antlr4.Runtime;

namespace Redchess.Pgn
{
    public interface IPgnProcessor
    {
        void ResetGame();
        void ProcessMove(IToken token, string promotedPiece, string checkOrMate, string annotationGlyph);
        void DoFen(string fen);
    }
}