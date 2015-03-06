using System;
using System.Linq;
using System.Text;
using NUnit.Framework;
using RedChess.ChessCommon.Enumerations;
using Redchess.Engine;
using Redchess.Engine.Interfaces;

namespace Redchess.EngineTests
{
    internal sealed class InteractiveBoard : Board
    {
        public InteractiveBoard(PieceColor color, bool isEmpty) : base(color, isEmpty)
        {
        }

        public override void FromFen(string fen)
        {
            Console.WriteLine("Creating board from {0}\r\n", fen);
            base.FromFen(fen);
            Console.WriteLine(this);
        }

        public override bool Move(Location start, Location end)
        {
            bool success = base.Move(start, end);
           // Console.WriteLine("Move is annotated as {0}", LastMove());
            Assert.IsTrue(success, "Expected move from {0} to {1} to succeed", start, end);
            Console.WriteLine("Moved from {0} to {1}", start, end);
            Console.WriteLine(this);
            return true;
        }

        public bool MoveExpectFailure(Location start, Location end)
        {
            bool success = base.Move(start, end);
            Assert.IsFalse(success, "Expected move from {0} to {1} to fail", start, end);
            Console.WriteLine("Tried to move from {0} to {1} and failed as expected");
            return false;
        }

        public override string ToString()
        {
            var sb = new StringBuilder();
            var boardText = Enumerable.Repeat('.', 64).ToArray();
            var lineData = new char[8];

            for (int i = 0; i < 64; i++)
            {
                IPiece p = GetContents((Location) i);
                if (p != null)
                {
                    boardText[i] = PieceData.Symbol(p.Type)[0];
                }
                else
                {
                    if (EnPassantTarget == (Location) i)
                        boardText[i] = '*';
                }
            }

            for (int i = 7; i >= 0; i--)
            {
                Array.Copy(boardText, i*8, lineData, 0, 8);
                sb.Append(new string(lineData));
                sb.AppendLine();
            }

            return sb.ToString();
        }
    }
}