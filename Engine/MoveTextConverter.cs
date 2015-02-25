using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Policy;
using System.Text;
using System.Threading.Tasks;
using Redchess.Engine.Interfaces;
using RedChess.ChessCommon;
using RedChess.ChessCommon.Enumerations;

namespace Redchess.Engine
{
    internal class MoveTextConverter
    {
        private readonly IBoardExtended m_board;

        public MoveTextConverter(IBoardExtended game)
        {
            m_board = game;
        }

        public string MoveAsText(IPiece piece, Location newLocation)
        {
            if (piece.Type.IsOfType(PieceType.Pawn))
            {
                return PawnMove(piece, newLocation);
            }

            return "";
        }

        private string PawnMove(IPiece piece, Location newLocation)
        {
            if (m_board.GetContents(newLocation) != null)
            {
                return String.Format("{0}x{1}", "abcdefgh"[piece.Position.X], newLocation.ToString().ToLower());
            }

            return newLocation.ToString().ToLower();
        }
    }
}
