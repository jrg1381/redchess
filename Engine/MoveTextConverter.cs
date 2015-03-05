using System;
using System.Linq;
using System.Threading.Tasks;
using Redchess.Engine.Interfaces;
using Redchess.Engine.Structures;
using RedChess.ChessCommon;
using RedChess.ChessCommon.Enumerations;

namespace Redchess.Engine
{
    internal class MoveTextConverter
    {
        private readonly BoardWithNextMove m_moveToPlay;

        internal MoveTextConverter(BoardWithNextMove previousState)
        {
            m_moveToPlay = previousState;
        }

        internal string MoveAsText()
        {
            string answer;

            if (m_moveToPlay.MovedPiece.Type.IsOfType(PieceType.Pawn))
            {
                answer = m_moveToPlay.Promotion != null ? Promotion() : PawnMove();
            }
            else if (m_moveToPlay.MovedPiece.Type.IsOfType(PieceType.King))
            {
                answer = KingMove();
            }
            else
            {
                answer = PieceMove();
            }

            return answer + Annotation();
        }

        private string Promotion()
        {
            return String.Format("{0}(={1})",
                PawnMove().TrimEnd('+', '#'),
                m_moveToPlay.Promotion);
        }

        private string KingMove()
        {
            int dX = new Square(m_moveToPlay.Target).X - m_moveToPlay.MovedPiece.Position.X;

            switch (dX)
            {
                case 2:
                    return "O-O";
                case -2:
                    return "O-O-O";
                default:
                    return PieceMove();
            }
        }

        private string PawnMove()
        {
            var piece = m_moveToPlay.MovedPiece;
            var newLocation = m_moveToPlay.Target;

            if (m_moveToPlay.Board.GetContents(newLocation) != null || m_moveToPlay.Board.EnPassantTarget == newLocation)
            {
                return String.Format("{0}x{1}", PieceColumn(piece), 
                    LocationToLower(newLocation));
            }

            return LocationToLower(newLocation);
        }

        private string PieceMove()
        {
            var piece = m_moveToPlay.MovedPiece;
            var newLocation = m_moveToPlay.Target;
            var disambiguator = Disambiguator();

            if (m_moveToPlay.Board.GetContents(newLocation) != null)
            {
                return String.Format("{0}{1}x{2}", 
                    PieceSymbol(piece), 
                    disambiguator,
                    LocationToLower(newLocation));
            }

            return String.Format("{0}{1}{2}",
                PieceSymbol(piece), 
                disambiguator,
                LocationToLower(newLocation));
        }

        private string Disambiguator()
        {
            var disambiguator = String.Empty;
            var piece = m_moveToPlay.MovedPiece;

            if (MoveIsAmbiguous())
            {
                if (MoveIsAmbiguousWithColumn())
                {
                    if (MoveIsAmbiguousWithRow())
                    {
                        disambiguator = LocationToLower(piece.Position.Location);
                    }
                    else
                    {
                        disambiguator = (piece.Position.Y + 1).ToString();
                    }
                }
                else
                {
                    disambiguator = PieceColumn(piece);
                }
            }

            return disambiguator;
        }

        private bool MoveIsAmbiguous()
        {
            var piece = m_moveToPlay.MovedPiece;
            var newLocation = m_moveToPlay.Target;

            return m_moveToPlay.Board.FindPieces(piece.Type).Where(p => p != piece.Position.Location).
                Any(p => m_moveToPlay.Board.GetContents(p).ValidMoves(m_moveToPlay.Board).Contains(newLocation));
        }

        private bool MoveIsAmbiguousWithColumn()
        {
            var piece = m_moveToPlay.MovedPiece;
            var newLocation = m_moveToPlay.Target;

            return m_moveToPlay.Board.FindPieces(piece.Type).Where(p => p != piece.Position.Location && (new Square(p)).X == piece.Position.X).
                Any(p => m_moveToPlay.Board.GetContents(p).ValidMoves(m_moveToPlay.Board).Contains(newLocation));
        }

        private bool MoveIsAmbiguousWithRow()
        {
            var piece = m_moveToPlay.MovedPiece;
            var newLocation = m_moveToPlay.Target;

            return m_moveToPlay.Board.FindPieces(piece.Type).Where(p => p != piece.Position.Location && (new Square(p)).Y == piece.Position.Y).
                Any(p => m_moveToPlay.Board.GetContents(p).ValidMoves(m_moveToPlay.Board).Contains(newLocation));
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

        private string Annotation()
        {
            var piece = m_moveToPlay.MovedPiece;
            var newLocation = m_moveToPlay.Target;

            var boardCopy = new Board();
            boardCopy.FromFen(m_moveToPlay.Board.ToFen());
            boardCopy.Move(piece.Position.Location, newLocation);
            if(m_moveToPlay.Promotion != null)
                boardCopy.PromotePiece(m_moveToPlay.Promotion);

            var isCheck = boardCopy.KingInCheck();
            var isMate = isCheck && boardCopy.IsCheckmate(skipCheckTest: true);

            if (isMate) return "#";
            if (isCheck) return "+";

            return String.Empty;
        }
    }
}
