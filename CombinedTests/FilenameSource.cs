using System.Collections.Generic;
using System.IO;

namespace RedChess.CombinedTests
{
    public class FilenameSource
    {
        public IEnumerable<string> PgnFile => Directory.GetFiles(@".\PgnFiles", "*.pgn");
    }
}