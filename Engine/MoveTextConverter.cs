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
        private readonly BoardStateTransition m_MoveToPlay;
        private Task<string> m_DisambiguatorTask;

        private string DisambiguatorText
        {
            get
            {
                if (m_DisambiguatorTask == null)
                    return String.Empty;

                return m_DisambiguatorTask.ConfigureAwait(false).GetAwaiter().GetResult();
            }
        }

        internal MoveTextConverter(BoardStateTransition previousState)
        {
            m_MoveToPlay = previousState;
        }

        internal string MoveAsText()
        {
            return MoveAsTextAsync().Result;
        }

        private async Task<string> MoveAsTextAsync()
        {
            var annotationTask = AnnotationTask().ConfigureAwait(false);
            string answer;

            if (m_MoveToPlay.MovedPiece.Type.IsOfType(PieceType.Pawn))
            {
                answer = m_MoveToPlay.Promotion != null ? Promotion() : PawnMove();
            }
            else if (m_MoveToPlay.MovedPiece.Type.IsOfType(PieceType.King))
            {
                answer = KingMove();
            }
            else
            {
                m_DisambiguatorTask = DisambiguatorTask();
                answer = PieceMove();
            }

            return answer + await annotationTask;
        }

        private string Promotion()
        {
            return $"{PawnMove()}={m_MoveToPlay.Promotion}";
        }

        private string KingMove()
        {
            int dX = new Square(m_MoveToPlay.Target).X - m_MoveToPlay.MovedPiece.Position.X;

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
            var piece = m_MoveToPlay.MovedPiece;
            var newLocation = m_MoveToPlay.Target;

            if (m_MoveToPlay.BoardBefore.GetContents(newLocation) != null || m_MoveToPlay.BoardBefore.EnPassantTarget == newLocation)
            {
                return $"{PieceColumn(piece)}x{LocationToLower(newLocation)}";
            }

            return LocationToLower(newLocation);
        }

        private string PieceMove()
        {
            var piece = m_MoveToPlay.MovedPiece;
            var newLocation = m_MoveToPlay.Target;

            if (m_MoveToPlay.BoardBefore.GetContents(newLocation) != null)
            {
                return $"{PieceSymbol(piece)}{DisambiguatorText}x{LocationToLower(newLocation)}";
            }

            return $"{PieceSymbol(piece)}{DisambiguatorText}{LocationToLower(newLocation)}";
        }

        private Task<string> DisambiguatorTask()
        {
            return Task.Run(() =>
            {
                var disambiguator = String.Empty;
                var piece = m_MoveToPlay.MovedPiece;

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
            var piece = m_MoveToPlay.MovedPiece;
            var newLocation = m_MoveToPlay.Target;

            return m_MoveToPlay.BoardBefore.FindPieces(piece.Type).Where(p => p != piece.Position.Location).
                Any(p => m_MoveToPlay.BoardBefore.GetContents(p).ValidMoves(m_MoveToPlay.BoardBefore).Contains(newLocation));
        }

        private bool MoveIsAmbiguousWithColumn()
        {
            var piece = m_MoveToPlay.MovedPiece;
            var newLocation = m_MoveToPlay.Target;

            return m_MoveToPlay.BoardBefore.FindPieces(piece.Type).Where(p => p != piece.Position.Location && new Square(p).X == piece.Position.X).
                Any(p => m_MoveToPlay.BoardBefore.GetContents(p).ValidMoves(m_MoveToPlay.BoardBefore).Contains(newLocation));
        }

        private bool MoveIsAmbiguousWithRow()
        {
            var piece = m_MoveToPlay.MovedPiece;
            var newLocation = m_MoveToPlay.Target;

            return m_MoveToPlay.BoardBefore.FindPieces(piece.Type).Where(p => p != piece.Position.Location && new Square(p).Y == piece.Position.Y).
                Any(p => m_MoveToPlay.BoardBefore.GetContents(p).ValidMoves(m_MoveToPlay.BoardBefore).Contains(newLocation));
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
                if (m_MoveToPlay.BoardAfter.Check)
                {
                    var gameStatus = m_MoveToPlay.BoardAfter.StatusForBoard();
                    if (gameStatus == GameStatus.CheckmateBlackWins || gameStatus == GameStatus.CheckmateWhiteWins) return "#";
                    return "+";
                }

                return String.Empty;
            });
        }
    }
}
