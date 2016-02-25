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
        private readonly BoardStateTransition m_moveToPlay;
        private Task<string> m_disambiguatorTask;

        private string DisambiguatorText
        {
            get
            {
                if (m_disambiguatorTask == null)
                    return String.Empty;

                return m_disambiguatorTask.ConfigureAwait(false).GetAwaiter().GetResult();
            }
        }

        internal MoveTextConverter(BoardStateTransition previousState)
        {
            m_moveToPlay = previousState;
        }

        internal string MoveAsText()
        {
            return MoveAsTextAsync().Result;
        }

        private async Task<string> MoveAsTextAsync()
        {
            var annotationTask = AnnotationTask().ConfigureAwait(false);
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
                m_disambiguatorTask = DisambiguatorTask();
                answer = PieceMove();
            }

            return answer + await annotationTask;
        }

        private string Promotion()
        {
            return String.Format("{0}={1}",
                PawnMove(),
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

            if (m_moveToPlay.BoardBefore.GetContents(newLocation) != null || m_moveToPlay.BoardBefore.EnPassantTarget == newLocation)
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

            if (m_moveToPlay.BoardBefore.GetContents(newLocation) != null)
            {
                return String.Format("{0}{1}x{2}", 
                    PieceSymbol(piece),
                    DisambiguatorText,
                    LocationToLower(newLocation));
            }

            return String.Format("{0}{1}{2}",
                PieceSymbol(piece),
                DisambiguatorText,
                LocationToLower(newLocation));
        }

        private Task<string> DisambiguatorTask()
        {
            return Task.Run(() =>
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
            });
        }

        private bool MoveIsAmbiguous()
        {
            var piece = m_moveToPlay.MovedPiece;
            var newLocation = m_moveToPlay.Target;

            return m_moveToPlay.BoardBefore.FindPieces(piece.Type).Where(p => p != piece.Position.Location).
                Any(p => m_moveToPlay.BoardBefore.GetContents(p).ValidMoves(m_moveToPlay.BoardBefore).Contains(newLocation));
        }

        private bool MoveIsAmbiguousWithColumn()
        {
            var piece = m_moveToPlay.MovedPiece;
            var newLocation = m_moveToPlay.Target;

            return m_moveToPlay.BoardBefore.FindPieces(piece.Type).Where(p => p != piece.Position.Location && (new Square(p)).X == piece.Position.X).
                Any(p => m_moveToPlay.BoardBefore.GetContents(p).ValidMoves(m_moveToPlay.BoardBefore).Contains(newLocation));
        }

        private bool MoveIsAmbiguousWithRow()
        {
            var piece = m_moveToPlay.MovedPiece;
            var newLocation = m_moveToPlay.Target;

            return m_moveToPlay.BoardBefore.FindPieces(piece.Type).Where(p => p != piece.Position.Location && (new Square(p)).Y == piece.Position.Y).
                Any(p => m_moveToPlay.BoardBefore.GetContents(p).ValidMoves(m_moveToPlay.BoardBefore).Contains(newLocation));
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

        private Task<string> AnnotationTask()
        {
            return Task.Run(() =>
            {
                if (m_moveToPlay.BoardAfter.Check)
                {
                    var gameStatus = m_moveToPlay.BoardAfter.StatusForBoard();
                    if (gameStatus == GameStatus.CheckmateBlackWins || gameStatus == GameStatus.CheckmateWhiteWins) return "#";
                    return "+";
                }

                return String.Empty;
            });
        }
    }
}
