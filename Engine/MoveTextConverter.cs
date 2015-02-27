using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Security.Policy;
using System.Text;
using System.Threading.Tasks;
using Redchess.Engine.Interfaces;
using Redchess.Engine.Structures;
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
            if (MoveIsAmbiguous(piece, newLocation))
            {
                throw new ArgumentException("Move is ambiguous");
            }

            if (piece.Type.IsOfType(PieceType.Pawn))
            {
                return PawnMove(piece, newLocation);
            }
            else if (piece.Type.IsOfType(PieceType.King))
            {
                return KingMove(piece, newLocation);
            }
            else
            {
                return PieceMove(piece, newLocation);
            }

            return "";
        }

        private string KingMove(IPiece piece, Location newLocation)
        {
            int dX = new Square(newLocation).X - piece.Position.X;

            switch (dX)
            {
                case 2:
                    return "O-O";
                case -2:
                    return "O-O-O";
                default:
                    return PieceMove(piece, newLocation) + Annotation(piece, newLocation);
            }
        }

        private string PawnMove(IPiece piece, Location newLocation)
        {
            if (m_board.GetContents(newLocation) != null)
            {
                return String.Format("{0}x{1}{2}", PieceColumn(piece), LocationToLower(newLocation), Annotation(piece, newLocation));
            }

            return LocationToLower(newLocation) + Annotation(piece, newLocation);
        }

        private string PieceMove(IPiece piece, Location newLocation)
        {
            if (m_board.GetContents(newLocation) != null)
            {
                return String.Format("{0}x{1}{2}", 
                    PieceSymbol(piece), 
                    LocationToLower(newLocation), 
                    Annotation(piece, newLocation));
            }

            return String.Format("{0}{1}{2}",
                PieceSymbol(piece), 
                LocationToLower(newLocation), 
                Annotation(piece, newLocation));
        }

        private bool MoveIsAmbiguous(IPiece piece, Location newLocation)
        {
            var otherPieces = m_board.FindPieces(piece.Type).Where(p => p != piece.Position.Location);
            var movesOfOtherPieces = otherPieces.SelectMany(p => m_board.GetContents(p).ValidMoves(m_board));
            return movesOfOtherPieces.Contains(newLocation);
        }

        private string LocationToLower(Location location)
        {
            return location.ToString().ToLower();
        }

        private string PieceColumn(IPiece piece)
        {
            return "abcdefgh"[piece.Position.X].ToString();
        }

        private string PieceSymbol(IPiece piece)
        {
            return PieceData.Symbol(piece.Type).ToUpper();
        }

        private string Annotation(IPiece piece, Location newLocation)
        {
            var boardCopy = new Board();
            boardCopy.FromFen(m_board.ToFen());
            boardCopy.Move(piece.Position.Location, newLocation);

            var isCheck = boardCopy.KingInCheck();
            var isMate = isCheck && boardCopy.IsCheckmate(skipCheckTest: true);

            if (isMate) return "#";
            if (isCheck) return "+";

            return String.Empty;
        }
    }
}
