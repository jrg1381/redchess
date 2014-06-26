using RedChess.ChessCommon.Interfaces;
using RedChess.PgnProcessor;

namespace RedChess.ParserFactory
{
    public static class ParserFactory
    {
        public static IParse GetParser()
        {
            return new PgnParserImpl();
        }
    }
}