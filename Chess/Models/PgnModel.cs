using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using RedChess.ChessCommon;
using RedChess.ChessCommon.Enumerations;

namespace Chess.Models
{
    public struct FenWithMove
    {
        public string Fen { get; set; }
        public string Move { get; set; }
    }

    public class PgnModel
    {
        public List<string> ErrorText { get; private set; }
        public List<FenWithMove> Moves { get; private set; } 
        public Dictionary<string, string> Tags { get; private set; }

        public PgnModel()
        {
            ErrorText = new List<string>();
            Moves = new List<FenWithMove>();
            Tags = new Dictionary<string, string>();
        }

        internal void RecordMove(string fen, string moveText, ChessMove moveMade)
        {
            var fenWithMove = new FenWithMove {Fen = fen, Move = moveText};
            Moves.Add(fenWithMove);
        }
    }
}