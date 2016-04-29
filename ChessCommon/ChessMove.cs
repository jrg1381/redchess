using RedChess.ChessCommon.Enumerations;

namespace RedChess.ChessCommon
{
    public class ChessMove
    {
        public Location Start { get; private set; }
        public Location End { get; private set; }
        public string Promotion { get; private set; }

        public ChessMove(Location start, Location end, string promotion = null)
        {
            Start = start;
            End = end;
            Promotion = promotion;
        }
    }
}
