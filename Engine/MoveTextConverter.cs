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
        public MoveTextConverter(IBoardExtended x)
        {
        }

        public string MoveAsText(BoardWithNextMove previousState)
        {
            if (previousState.MovedPiece.Type.IsOfType(PieceType.Pawn))
            {
                if (previousState.Promotion != null)
                    return Promotion(previousState);

                return PawnMove(previousState);
            }
            else if (previousState.MovedPiece.Type.IsOfType(PieceType.King))
            {
                return KingMove(previousState);
            }
            else
            {
                return PieceMove(previousState);
            }

            return "";
        }

        private string Promotion(BoardWithNextMove previousState)
        {
            return String.Format("{0}(={1}){2}",
                PawnMove(previousState).TrimEnd('+', '#'),
                previousState.Promotion,
                Annotation(previousState));
        }

        private string KingMove(BoardWithNextMove previousState)
        {
            int dX = new Square(previousState.Target).X - previousState.MovedPiece.Position.X;

            switch (dX)
            {
                case 2:
                    return "O-O" + Annotation(previousState);
                case -2:
                    return "O-O-O" + Annotation(previousState);
                default:
                    return PieceMove(previousState);
            }
        }

        private string PawnMove(BoardWithNextMove previousState)
        {
            var piece = previousState.MovedPiece;
            var newLocation = previousState.Target;

            if (previousState.Board.GetContents(newLocation) != null || previousState.Board.EnPassantTarget == newLocation)
            {
                return String.Format("{0}x{1}{2}", PieceColumn(piece), 
                    LocationToLower(newLocation),
                    Annotation(previousState));
            }

            return LocationToLower(newLocation) + Annotation(previousState);
        }

        private string PieceMove(BoardWithNextMove previousState)
        {
            var piece = previousState.MovedPiece;
            var newLocation = previousState.Target;
            var disambiguator = Disambiguator(previousState);

            if (previousState.Board.GetContents(newLocation) != null)
            {
                return String.Format("{0}{1}x{2}{3}", 
                    PieceSymbol(piece), 
                    disambiguator,
                    LocationToLower(newLocation),
                    Annotation(previousState));
            }

            return String.Format("{0}{1}{2}{3}",
                PieceSymbol(piece), 
                disambiguator,
                LocationToLower(newLocation),
                Annotation(previousState));
        }

        private string Disambiguator(BoardWithNextMove previousState)
        {
            var disambiguator = String.Empty;
            var piece = previousState.MovedPiece;

            if (MoveIsAmbiguous(previousState))
            {
                if (MoveIsAmbiguousWithColumn(previousState))
                {
                    if (MoveIsAmbiguousWithRow(previousState))
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

        private bool MoveIsAmbiguous(BoardWithNextMove previousState)
        {
            var piece = previousState.MovedPiece;
            var newLocation = previousState.Target;

            return previousState.Board.FindPieces(piece.Type).Where(p => p != piece.Position.Location).
                Any(p => previousState.Board.GetContents(p).ValidMoves(previousState.Board).Contains(newLocation));
        }

        private bool MoveIsAmbiguousWithColumn(BoardWithNextMove previousState)
        {
            var piece = previousState.MovedPiece;
            var newLocation = previousState.Target;

            return previousState.Board.FindPieces(piece.Type).Where(p => p != piece.Position.Location && (new Square(p)).X == piece.Position.X).
                Any(p => previousState.Board.GetContents(p).ValidMoves(previousState.Board).Contains(newLocation));
        }

        private bool MoveIsAmbiguousWithRow(BoardWithNextMove previousState)
        {
            var piece = previousState.MovedPiece;
            var newLocation = previousState.Target;

            return previousState.Board.FindPieces(piece.Type).Where(p => p != piece.Position.Location && (new Square(p)).Y == piece.Position.Y).
                Any(p => previousState.Board.GetContents(p).ValidMoves(previousState.Board).Contains(newLocation));
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

        private string Annotation(BoardWithNextMove previousState)
        {
            var piece = previousState.MovedPiece;
            var newLocation = previousState.Target;

            var boardCopy = new Board();
            boardCopy.FromFen(previousState.Board.ToFen());
            boardCopy.Move(piece.Position.Location, newLocation);
            if(previousState.Promotion != null)
                boardCopy.PromotePiece(previousState.Promotion);

            var isCheck = boardCopy.KingInCheck();
            var isMate = isCheck && boardCopy.IsCheckmate(skipCheckTest: true);

            if (isMate) return "#";
            if (isCheck) return "+";

            return String.Empty;
        }
    }
}
