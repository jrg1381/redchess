using RedChess.ChessCommon.Interfaces;
using Redchess.Engine;

namespace RedChess.EngineFactory
{
    public static class BoardFactory
    {
        public static IBoard CreateInstance()
        {
            return new Board();
        }
    }
}
