using System.Collections.Generic;
using System.IO;

namespace CombinedTests
{
    public class FilenameSource
    {
        public IEnumerable<string> PgnFile
        {
            get
            {
                return Directory.GetFiles(@".\PgnFiles", "*.pgn");
            }
        }
    }
}