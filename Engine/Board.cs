using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using RedChess.ChessCommon;
using RedChess.ChessCommon.Enumerations;
using RedChess.ChessCommon.Interfaces;
using Redchess.Engine.Exceptions;
using Redchess.Engine.Interfaces;
using Redchess.Engine.Pieces;
using Redchess.Engine.Pieces.Abstract;
using Redchess.Engine.Structures;

namespace Redchess.Engine
{
    public class Board : IBoard, IBoardExtended
    {
        private static readonly int s_parallelism = Environment.ProcessorCount;

        public PieceColor CurrentTurn { get; private set; }
        private readonly Fen m_fen;
        private Pawn m_promotedPawn;
        private Location m_enPassantTarget;
        private readonly CastlingRules m_castlingRules;
        protected SimpleBoard SimpleBoard;

        public string ToFen()
        {
            return m_fen.ToFen();
        }

        public void FromFen(string fen)
        {
            string[] fields = fen.Split(new[] {' '});
            string position = fields[0];
            string currentTurn = fields[1];
            string castling = fields[2];
            string enPassantTarget = fields[3];
            int index = 0;

            CurrentTurn = currentTurn == "b" ? PieceColor.Black : PieceColor.White;
            m_castlingRules.UpdateFromFen(castling);

            if (enPassantTarget != "-")
            {
                m_enPassantTarget = (Location) Enum.Parse(typeof (Location), enPassantTarget.ToUpper());
            }

            SimpleBoard = new SimpleBoard(true);

            foreach (char c in position)
            {
                // A1 is square 0, H8 is square 63, but FEN reads from L->R, top to bottom
                int trueIndex = 8*(7 - index/8) + (index%8);
                if (Char.IsDigit(c))
                {
                    index += Int32.Parse(c.ToString(CultureInfo.InvariantCulture));
                    continue;
                }

                if (c == '/')
                {
                    continue;
                }

                var t = PieceData.TypeFromSymbol(c);
                SimpleBoard.AddPiece(PieceFactory.CreatePiece(t, (Location) trueIndex));
                index++;
            }
        }

        public bool Move(Location start, Location end)
        {
            var piece = GetContents(start);

            if (piece == null || piece.Color != CurrentTurn || start == end)
                return false;

            if (!piece.ReachableSquares(this).Contains(end))
                return false;

            if (!ValidateMoveForCheck(piece, end))
                return false;

            MovePiece(piece, end);
            return true;
        }

        public Board()
            : this(PieceColor.White, false, true)
        {
        }

        protected Board(PieceColor whoseTurn = PieceColor.White, bool isEmpty = false, bool createNewSimpleBoard = true)
        {
            CurrentTurn = whoseTurn;
            m_castlingRules = new CastlingRules();
            m_enPassantTarget = Location.InvalidSquare;
            m_fen = new Fen(this, m_castlingRules);
            if (createNewSimpleBoard)
                SimpleBoard = new SimpleBoard(isEmpty);
        }

        // True if the move will not put (or leave) the king in check
        public bool ValidateMoveForCheck(IPiece piece, Location newLocation)
        {
            var boardCopy = DeepClone();
            boardCopy.MovePiece(piece, newLocation);

            // Note that CurrentTurn comes from the current board, not boardCopy, because MovePiece changes the turn...
            // Note that KingPosition comes from the boardCopy, because the move might have moved the king...
            return !boardCopy.KingInCheck(CurrentTurn, boardCopy.KingPosition(CurrentTurn));
        }

        public IPiece GetContents(Location loc)
        {
            return SimpleBoard.GetContents(loc);
        }

        public Location EnPassantTarget
        {
            get { return m_enPassantTarget; }
        }

        public bool IsAwaitingPromotionDecision()
        {
            return m_promotedPawn != null;
        }

        /// <summary>
        ///     Returns true if the position is checkmate
        /// </summary>
        /// <param name="skipCheckTest">Set to true to omit the initial test for the king being in check</param>
        /// <returns></returns>
        public bool IsCheckmate(bool skipCheckTest = false)
        {
            if (!skipCheckTest && !KingInCheck())
                return false; // Can't be mate if the king is not in check

            // The king is in check, and..
            return !ValidMovesExist();
        }

        public IEnumerable<Location> FindPieces(PieceType pieceType)
        {
            return SimpleBoard.OccupiedSquares().Where(x => GetContents(x).Type == pieceType);
        }

        private bool ValidMovesExist()
        {
            var friends = SimpleBoard.Pieces(CurrentTurn).OccupiedSquares().Select(GetContents);

            return friends.AsParallel().Any(x => x.ValidMoves(this).Any());
        }

        public bool IsStalemate()
        {
            return (!KingInCheck() && !ValidMovesExist());
        }

        public bool IsDraw()
        {
            int totalPieceCount = SimpleBoard.OccupiedSquares().Count();

            if (totalPieceCount > 3) return false; // There are enough pieces for it not to be an obvious draw

            if (totalPieceCount == 2)
            {
                // King vs King
                return true;
            }

            if (SimpleBoard.PiecesOfType(PieceType.WhiteKnight) == 1 || SimpleBoard.PiecesOfType(PieceType.BlackKnight) == 1)
                return true;

            if (SimpleBoard.PiecesOfType(PieceType.WhiteBishop) == 1 || SimpleBoard.PiecesOfType(PieceType.BlackBishop) == 1)
                return true;

            return false;
        }

        internal void PromotePiece(PieceType promotionTarget)
        {
            if (m_promotedPawn == null)
                throw new CannotPromoteException();
            SimpleBoard.RemovePiece(m_promotedPawn);
            SimpleBoard.AddPiece(PieceFactory.CreatePiece(promotionTarget, m_promotedPawn.Position.Location));
            m_promotedPawn = null;
        }

        public void PromotePiece(string promotionTarget)
        {
            // Crappy, but we want to allow K (for Knight, when the UI asks for it) but not King.
            // We also want to work on the intial letters only, for when the parser reads a PGN file.
            if (promotionTarget.ToLower() == "king")
                throw new CannotPromoteException();

            char initialLetter = Char.ToUpper(promotionTarget[0]);

            if (!new[] {'Q', 'R', 'B', 'N', 'K'}.Contains(initialLetter))
                throw new CannotPromoteException();

            var flagsForPromotedPiece = PieceType.RealPiece;

            if (CurrentTurn == PieceColor.White)
                flagsForPromotedPiece |= PieceType.Black;
            else
                flagsForPromotedPiece |= PieceType.White;

            switch (initialLetter)
            {
                case 'Q':
                    PromotePiece(PieceType.Queen | flagsForPromotedPiece);
                    break;
                case 'R':
                    PromotePiece(PieceType.Rook | flagsForPromotedPiece);
                    break;
                case 'B':
                    PromotePiece(PieceType.Bishop | flagsForPromotedPiece);
                    break;
                case 'K':
                case 'N':
                    PromotePiece(PieceType.Knight | flagsForPromotedPiece);
                    break;
            }
        }

        internal void MovePiece(IPiece piece, Location newLocation)
        {
            if (m_promotedPawn != null)
                throw new InvalidMoveException("The previous player has not decided what to promote their pawn to");

            // Delete any piece on the target square
            var originalLocation = piece.Position;
            var originalOccupier = GetContents(newLocation);
            if (originalOccupier != null)
            {
                SimpleBoard.RemovePiece(originalOccupier);
                m_enPassantTarget = Location.InvalidSquare;
            }

            // Move the piece with no checking
            SimpleBoard.UnsafeMovePiece(ref piece, newLocation);

            if (piece.Type.IsOfType(PieceType.King))
            {
                int distanceMoved = (new Square(newLocation)).X - originalLocation.X;

                if (Math.Abs(distanceMoved) > 1)
                {
                    Castle(piece.Color, Math.Sign(distanceMoved) == 1 ? Side.KingSide : Side.QueenSide);
                }
            }

            if (piece.Type.IsOfType(PieceType.Pawn))
            {
                // If the target square was empty, could have been an e.p. capture
                MovePawn(piece as Pawn, newLocation, originalLocation);
            }
            else
            {
                m_enPassantTarget = Location.InvalidSquare;
            }

            m_castlingRules.Update(piece, originalLocation.Location, newLocation);
            CurrentTurn = ~CurrentTurn;
        }

        private void MovePawn(Pawn piece, Location newLocation, Square originalLocation)
        {
            var newSquare = new Square(newLocation);

            if ((newSquare.Y == 7 && CurrentTurn == PieceColor.White) ||
                (newSquare.Y == 0 && CurrentTurn == PieceColor.Black))
            {
                m_promotedPawn = piece;
            }

            int distanceMoved = newSquare.Y - originalLocation.Y;
            switch (distanceMoved)
            {
                case 2:
                    m_enPassantTarget = (new Square(originalLocation.X, originalLocation.Y + 1).Location);
                    return;
                case -2:
                    m_enPassantTarget = (new Square(originalLocation.X, originalLocation.Y - 1).Location);
                    return;
            }

            int horizontalDistanceMoved = newSquare.X - originalLocation.X;

            if (horizontalDistanceMoved != 0 && m_enPassantTarget != Location.InvalidSquare)
                // The pawn has taken something but there was nothing on the square where it took, i.e. EP
            {
                var epSquare = new Square(m_enPassantTarget);

                if (CurrentTurn == PieceColor.White)
                    SimpleBoard.RemovePiece(GetContents(new Square(epSquare.X, epSquare.Y - 1).Location));
                else
                    SimpleBoard.RemovePiece(GetContents(new Square(epSquare.X, epSquare.Y + 1).Location));
            }

            m_enPassantTarget = Location.InvalidSquare;
        }

        /// <summary>
        ///     Perform the Rook part of the castling manouevre
        /// </summary>
        /// <param name="pieceColor"></param>
        /// <param name="side"></param>
        private void Castle(PieceColor pieceColor, Side side)
        {
            if (pieceColor == PieceColor.White)
            {
                if (side == Side.KingSide)
                {
                    var piece = GetContents(Location.H1);
                    SimpleBoard.UnsafeMovePiece(ref piece, Location.F1);
                }
                else
                {
                    var piece = GetContents(Location.A1);
                    SimpleBoard.UnsafeMovePiece(ref piece, Location.D1);
                }
            }
            else
            {
                if (side == Side.KingSide)
                {
                    var piece = GetContents(Location.H8);
                    SimpleBoard.UnsafeMovePiece(ref piece, Location.F8);
                }
                else
                {
                    var piece = GetContents(Location.A8);
                    SimpleBoard.UnsafeMovePiece(ref piece, Location.D8);
                }
            }
        }

        public bool MayCastle(IPiece king, Side side)
        {
            return m_castlingRules.MayCastle(king.Color, side, this);
        }

        /// <summary>
        ///     Return a board bitmap blocked out with NullPieces representing all the pieces of the given colour
        /// </summary>
        /// <returns></returns>
        public IBoardBitmap Pieces(PieceColor color)
        {
            return SimpleBoard.Pieces(color);
        }

        private Location KingPosition(PieceColor colorOfKing)
        {
            // Crashes if there is no king with SequenceHasNoElements exception. This is deliberate, there should always be two kings.
            // Using FirstOrDefault will claim that the King is on A1, which is unhelpful.

            return SimpleBoard.OccupiedSquares().First(loc =>
            {
                var x = GetContents(loc);
                return x.Type.IsOfType(PieceType.King) && x.Color == colorOfKing;
            });
        }

        /// <summary>
        ///     Returns true if the king of the current player is in check right now
        /// </summary>
        /// <returns></returns>
        public bool KingInCheck()
        {
            return KingInCheck(CurrentTurn, KingPosition(CurrentTurn));
        }

        /// <summary>
        ///     Returns true if a king of color <paramref name="colorOfKing" /> placed on <paramref name="kingPosition" /> would be
        ///     in check
        /// </summary>
        /// <param name="colorOfKing"></param>
        /// <param name="kingPosition"></param>
        /// <returns></returns>
        public bool KingInCheck(PieceColor colorOfKing, Location kingPosition)
        {
            /* This is not an efficient algorithm for determining check, it goes through all the attackers and sees if they attack the king, stopping when one does.
			 * Way more efficient would be to track outwards from the king's position, looking to see if anything is attacking it. In the common case where the king
			 * is protected by a shield of its own pieces, this would terminate earlier once the search lines hit friendly pieces in all directions. */

            var fixedAttackers = SimpleBoard.Pieces(~colorOfKing).OccupiedSquares().Select(GetContents);

            return
                fixedAttackers.ToArray()
                    .AsParallel()
                    .WithDegreeOfParallelism(s_parallelism)
                    .Any(tmpPiece => tmpPiece.AttackedSquares(this).Contains(kingPosition));
        }

        private Board DeepClone()
        {
            // Important - does not copy the castling state because this is not required for determining check or not.
            var copy = new Board(CurrentTurn, isEmpty: true, createNewSimpleBoard: false)
            {
                SimpleBoard = SimpleBoard.DeepClone(),
                m_enPassantTarget = m_enPassantTarget
            };

            return copy;
        }
    }
}