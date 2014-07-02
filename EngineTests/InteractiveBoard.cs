using System;
using System.Linq;
using System.Text;
using RedChess.ChessCommon.Enumerations;
using RedChess.ChessCommon.Interfaces;
using Redchess.Engine.Interfaces;
using Redchess.Engine.Pieces;
using Redchess.Engine.Pieces.Abstract;

namespace Redchess.Engine
{
    internal sealed class InteractiveBoard : Board
    {
        /*
		public InteractiveBoard()
			: base(PieceColor.White, false)
		{} */

        public InteractiveBoard(PieceColor color, bool isEmpty) : base(color, isEmpty)
        {
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