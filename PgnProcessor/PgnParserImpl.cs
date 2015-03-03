using System;
using System.Collections.Generic;
using Antlr.Runtime;
using RedChess.ChessCommon.Interfaces;
using Redchess.Pgn;
using RedChess.ChessCommon.Enumerations;

namespace RedChess.PgnProcessor
{
    public class PgnParserImpl : IParse
    {
        private Dictionary<string, string> m_tags;

        public void Parse(string text, Action<string, string, Tuple<Location, Location>> onMoveAction, Action<string> onErrorAction, bool playGame = true)
        {
            m_tags = new Dictionary<string, string>();
            var processor = new PgnProcessor(onMoveAction);
            var lexer = new PgnLexer(new ANTLRStringStream(text), onErrorAction);
            var tokenStream = new CommonTokenStream(lexer);
            var parser = new PgnParser(tokenStream, processor, onErrorAction) {PlayGame = playGame};
            parser.parse();

            foreach (var kvp in parser.OptionalTags)
            {
                m_tags.Add(kvp.Key, kvp.Value);
            }

            m_tags["Black"] = parser.Black;
            m_tags["White"] = parser.White;

            Event = parser.Event;
            Result = parser.Result;
        }


        public string Event { get; set; }
        public string Result { get; set; }

        public IDictionary<string, string> Tags
        {
            get { return m_tags; }
        }
    }
}