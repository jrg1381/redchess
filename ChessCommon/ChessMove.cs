using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RedChess.ChessCommon.Enumerations;

namespace RedChess.ChessCommon
{
    public class ChessMove
    {
        public Location Start { get; set; }
        public Location End { get; set; }
        public string Promotion { get; set; }
    }
}
