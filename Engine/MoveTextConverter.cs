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
            if (m_board.GetContents(newLocation) != null || m_board.EnPassantTarget == newLocation)
            {
                return String.Format("{0}{1}x{2}{3}", PieceColumn(piece), 
                    Disambiguator(piece, newLocation),
                    LocationToLower(newLocation),
                    Annotation(piece, newLocation));
            }

            return LocationToLower(newLocation) + Annotation(piece, newLocation);
        }

        private string PieceMove(IPiece piece, Location newLocation)
        {
            var disambiguator = Disambiguator(piece, newLocation);

            if (m_board.GetContents(newLocation) != null)
            {
                return String.Format("{0}{1}x{2}{3}", 
                    PieceSymbol(piece), 
                    disambiguator,
                    LocationToLower(newLocation), 
                    Annotation(piece, newLocation));
            }

            return String.Format("{0}{1}{2}{3}",
                PieceSymbol(piece), 
                disambiguator,
                LocationToLower(newLocation), 
                Annotation(piece, newLocation));
        }

        private string Disambiguator(IPiece piece, Location newLocation)
        {
            string disambiguator = String.Empty;

            var anyAmbiguity = MoveIsAmbiguous(piece, newLocation);
            var sameRow = PiecesOnSameRow(piece, newLocation);
            var sameColumn = PiecesOnSameColumn(piece, newLocation);

            if (sameRow && sameColumn)
                disambiguator = LocationToLower(piece.Position.Location);
            else if (sameRow)
                disambiguator = PieceColumn(piece);
            else if (sameColumn)
                disambiguator = (piece.Position.Y + 1).ToString();
            else if (anyAmbiguity)
                disambiguator = PieceColumn(piece); // bishops and pawns
            return disambiguator;
        }

        private bool MoveIsAmbiguous(IPiece piece, Location newLocation)
        {
            return m_board.FindPieces(piece.Type).Where(p => p != piece.Position.Location).
                Any(p => m_board.GetContents(p).ValidMoves(m_board).Contains(newLocation));
        }

        private bool PiecesOnSameColumn(IPiece piece, Location newLocation)
        {
            var target = new Square(newLocation);

            return m_board.FindPieces(piece.Type).Where(p =>
            {
                var otherPiece = new Square(p);
                if (p == piece.Position.Location)
                    return false;
                return otherPiece.X == target.X && otherPiece.Y != target.Y;
            }).Any(p => m_board.GetContents(p).ValidMoves(m_board).Contains(newLocation));
        }

        private bool PiecesOnSameRow(IPiece piece, Location newLocation)
        {
            var target = new Square(newLocation);

            return m_board.FindPieces(piece.Type).Where(p =>
            {
                var otherPiece = new Square(p);
                if (p == piece.Position.Location)
                    return false;
                return otherPiece.Y == target.Y && otherPiece.X != target.X;
            }).Any(p => m_board.GetContents(p).ValidMoves(m_board).Contains(newLocation));
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
