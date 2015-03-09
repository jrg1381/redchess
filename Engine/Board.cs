using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Redchess.Engine.Exceptions;
using Redchess.Engine.Interfaces;
using Redchess.Engine.Pieces.Abstract;
using Redchess.Engine.Structures;
using RedChess.ChessCommon;
using RedChess.ChessCommon.Enumerations;
using RedChess.ChessCommon.Interfaces;

namespace Redchess.Engine
{
    public class Board : IBoardExtended, IDisposable
    {
        private static readonly int s_parallelism = Environment.ProcessorCount;

        private readonly CastlingRules m_castlingRules;
        private readonly Fen m_fen;
        private readonly MoveTranscriber m_transcriber;
        protected SimpleBoard SimpleBoard;
        private Location m_enPassantTarget;
        private Pawn m_promotedPawn;
        private List<IObserver<IBoardExtended>> m_observers;
        private readonly FiftyMoveRuleCounter m_fiftyMoveRule;

        public Board()
            : this(PieceColor.White, false, true)
        {
        }

        public BoardWithNextMove PreviousState { get; private set; }

        protected Board(PieceColor whoseTurn = PieceColor.White, bool isEmpty = false, bool createNewSimpleBoard = true)
        {
            m_observers = new List<IObserver<IBoardExtended>>();
            CurrentTurn = whoseTurn;
            m_enPassantTarget = Location.InvalidSquare;

            m_castlingRules = new CastlingRules(this);
            m_fiftyMoveRule = new FiftyMoveRuleCounter(this);

            if (createNewSimpleBoard)
            {
                SimpleBoard = new SimpleBoard(isEmpty);
                m_fen = new Fen(this, m_castlingRules);
                m_transcriber = new MoveTranscriber(this);
            }
        }

        public string LastMove()
        {
            return m_transcriber.LastMove();
        }

        public PieceColor CurrentTurn { get; private set; }

        public string ToFen()
        {
            return m_fen.ToFen();
        }

        public virtual void FromFen(string fen)
        {
            string[] fields = fen.Split(new[] {' '});
            string position = fields[0];
            string currentTurn = fields[1];
            string castling = fields[2];
            string enPassantTarget = fields[3];
            string halfMoveClock = fields[4];
            int index = 0;

            CurrentTurn = currentTurn == "b" ? PieceColor.Black : PieceColor.White;
            m_castlingRules.UpdateFromFen(castling);

            if (enPassantTarget != "-")
            {
                m_enPassantTarget = (Location) Enum.Parse(typeof (Location), enPassantTarget.ToUpper());
            }

            m_fiftyMoveRule.HalfMoveClock = Int32.Parse(halfMoveClock);

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
                SimpleBoard.AddPiece(t, (Location) trueIndex);
                index++;
            }

            m_fen.ForceFen(fen);
        }

        public virtual bool Move(Location start, Location end)
        {
            var piece = GetContents(start);

            if (piece == null || piece.Color != CurrentTurn || start == end)
                return false;

            if (!piece.ReachableSquares(this).Contains(end))
                return false;

            if (!ValidateMoveForCheck(piece, end))
                return false;

            var tmpBoard = DeepClone();

            PreviousState = new BoardWithNextMove
            {
                Board = tmpBoard,
                MovedPiece = piece,
                Target = end
            };

            MovePiece(piece, end);
            NotifyObservers();

            return true;
        }

        private void NotifyObservers()
        {
            if (m_observers == null) return;

            foreach (var o in m_observers)
                o.OnCompleted();
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

        public bool IsStalemate()
        {
            return (!KingInCheck() && !ValidMovesExist());
        }

        public bool IsDraw()
        {
            int totalPieceCount = SimpleBoard.PieceCount;

            if (totalPieceCount > 3) return false; // There are enough pieces for it not to be an obvious draw

            if (totalPieceCount == 2)
            {
                // King vs King
                return true;
            }

            if (SimpleBoard.PiecesOfType(PieceType.WhiteKnight) == 1 ||
                SimpleBoard.PiecesOfType(PieceType.BlackKnight) == 1)
                return true;

            if (SimpleBoard.PiecesOfType(PieceType.WhiteBishop) == 1 ||
                SimpleBoard.PiecesOfType(PieceType.BlackBishop) == 1)
                return true;

            return false;
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

            PreviousState.Promotion = initialLetter.ToString();

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

            NotifyObservers();
        }

        public int FiftyMoveCounter { get { return m_fiftyMoveRule.HalfMoveClock; } }

        /// <summary>
        ///     Returns true if the king of the current player is in check right now
        /// </summary>
        /// <returns></returns>
        public bool KingInCheck()
        {
            return KingInCheck(CurrentTurn, KingPosition(CurrentTurn));
        }

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

        public bool MayCastle(IPiece king, Side side)
        {
            return m_castlingRules.MayCastle(king.Color, side);
        }

        /// <summary>
        ///     Return a board bitmap blocked out with NullPieces representing all the pieces of the given colour
        /// </summary>
        /// <returns></returns>
        public IBoardBitmap Pieces(PieceColor color)
        {
            return SimpleBoard.Pieces(color);
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

            var fixedAttackers = SimpleBoard.Pieces(~colorOfKing).OccupiedSquares().Select(GetContents).ToArray();

            return
                fixedAttackers
                    .AsParallel()
                    .WithDegreeOfParallelism(s_parallelism)
                    .SelectMany(p => p.AttackedSquares(this))
                    .Any(s => s == kingPosition);
        }

        private bool ValidMovesExist()
        {
            var friends = SimpleBoard.Pieces(CurrentTurn).OccupiedSquares().Select(GetContents).ToArray();

            return friends
                .AsParallel()
                .WithDegreeOfParallelism(s_parallelism)
                .SelectMany(x => x.ValidMoves(this))
                .Any();
        }

        void PromotePiece(PieceType promotionTarget)
        {
            if (m_promotedPawn == null)
                throw new CannotPromoteException();
            SimpleBoard.RemovePiece(m_promotedPawn);
            SimpleBoard.AddPiece(promotionTarget, m_promotedPawn.Position.Location);

            m_promotedPawn = null;
        }

        void MovePiece(IPiece piece, Location newLocation)
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

            var verticalDistanceMoved = newSquare.Y - originalLocation.Y;
            if(Math.Abs(verticalDistanceMoved) > 1)
            {
                    m_enPassantTarget = (new Square(originalLocation.X, originalLocation.Y + Math.Sign(verticalDistanceMoved)).Location);
                    return;
            }

            var horizontalDistanceMoved = newSquare.X - originalLocation.X;
            if (horizontalDistanceMoved != 0 && m_enPassantTarget != Location.InvalidSquare)
                // The pawn has taken something but there was nothing on the square where it took, i.e. EP
            {
                var epSquare = new Square(m_enPassantTarget);
                // Note sign. If our pawn moved forward (white) then the e.p. target is back down the board.
                SimpleBoard.RemovePiece(GetContents(new Square(epSquare.X, epSquare.Y - verticalDistanceMoved).Location));
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

        private Location KingPosition(PieceColor colorOfKing)
        {
            var king = colorOfKing == PieceColor.Black ? PieceType.BlackKing : PieceType.WhiteKing;
            // Crashes if there is no king with SequenceHasNoElements exception. This is deliberate, there should always be two kings.
            // Using FirstOrDefault will claim that the King is on A1, which is unhelpful.
            return FindPieces(king).First();
        }

        private Board DeepClone()
        {
            var copy = new Board(CurrentTurn)
            {
                SimpleBoard = SimpleBoard.DeepClone(),
                m_enPassantTarget = m_enPassantTarget,
            };

            copy.m_castlingRules.UpdateFromFen(m_castlingRules.FenCastleString());
            return copy;
        }

        public IDisposable Subscribe(IObserver<IBoardExtended> observer)
        {
            m_observers.Add(observer);
            return new Unsubscriber(this);
        }

        sealed class Unsubscriber : IDisposable
        {
            private readonly Board m_board;

            internal Unsubscriber(Board b)
            {
                m_board = b;
            }

            public void Dispose()
            {
                m_board.m_observers = null;
            }
        }

        public void Dispose()
        {
            if(m_fen != null)
                m_fen.Dispose();
            if(m_transcriber != null)
                m_transcriber.Dispose();
            if(m_castlingRules != null)
                m_castlingRules.Dispose();
        }
    }
}