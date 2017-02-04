using System.Collections.Generic;
using System.IO;

namespace RedChess.ParserTests
{
    public class FilenameSource
    {
        public IEnumerable<string> PgnFile => Directory.GetFiles(@".\PgnFiles","*.pgn");
    }
}