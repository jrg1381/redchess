using RedChess.ChessCommon.Enumerations;
using Redchess.Engine.Interfaces;

namespace Redchess.Engine
{
    public class BoardWithNextMove
    {
        public IPiece MovedPiece { get; set; }
        public Location Target { get; set; }
        public IBoardExtended Board { get; set; }
        public string Promotion { get; set; }
    }
}