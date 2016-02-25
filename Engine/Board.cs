using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using log4net.Core;
using Redchess.Engine.Exceptions;
using Redchess.Engine.Interfaces;
using Redchess.Engine.Observers;
using Redchess.Engine.Pieces;
using Redchess.Engine.Pieces.Abstract;
using Redchess.Engine.Structures;
using RedChess.ChessCommon;
using RedChess.ChessCommon.Enumerations;

namespace Redchess.Engine
{
    public class Board : IBoardExtended
    {
        private static readonly int s_parallelism = Environment.ProcessorCount;
        // Observers
        private readonly TransientCastlingRules m_transientCastlingRules;
        private readonly PermanentCastlingRules m_permanentCastlingRules;
        private readonly Fen m_fen;
        private readonly MoveTranscriber m_transcriber;
        private readonly CheckCacheCurrentPlayer m_checkCacheCurrentPlayer;
        private readonly CheckCacheOpposingPlayer m_checkCacheOtherPlayer;
        private readonly FiftyMoveRuleCounter m_fiftyMoveRule;

        private Pawn m_promotedPawn;
        private readonly List<IObserver<IBoardExtended>> m_observers = new List<IObserver<IBoardExtended>>();
        protected SimpleBoard SimpleBoard { get; set; }

        public BoardStateTransition PreviousState { get; private set; }

        public string FenCastleString()
        {
            return m_permanentCastlingRules.Value.Fen();
        }

        public CastlingOptions PermanentCastlingOptions
        {
            get { return m_permanentCastlingRules.Value; }
        }

        // Copy constructor
        internal Board(Board replacementBoard)
        {
            CurrentTurn = replacementBoard.CurrentTurn;
            EnPassantTarget = replacementBoard.EnPassantTarget;

            SimpleBoard = replacementBoard.SimpleBoard.DeepClone();

            m_permanentCastlingRules = new PermanentCastlingRules(this, replacementBoard);
            m_transientCastlingRules = new TransientCastlingRules(this);
            m_fiftyMoveRule = new FiftyMoveRuleCounter(this, replacementBoard);
            m_fen = new Fen(this);
            m_transcriber = new MoveTranscriber(this);
            m_checkCacheCurrentPlayer = new CheckCacheCurrentPlayer(this);
            m_checkCacheOtherPlayer = new CheckCacheOpposingPlayer(this);
        }

        public Board(PieceColor whoseTurn = PieceColor.White, bool isEmpty = false)
        {
            CurrentTurn = whoseTurn;
            EnPassantTarget = Location.InvalidSquare;

            SimpleBoard = new SimpleBoard(isEmpty);

            m_permanentCastlingRules = new PermanentCastlingRules(this);
            m_transientCastlingRules = new TransientCastlingRules(this);
            m_fiftyMoveRule = new FiftyMoveRuleCounter(this);
            m_fen = new Fen(this);
            m_transcriber = new MoveTranscriber(this);
            m_checkCacheCurrentPlayer = new CheckCacheCurrentPlayer(this);
            m_checkCacheOtherPlayer = new CheckCacheOpposingPlayer(this);
        }

        public string LastMove()
        {
            return m_transcriber.Value;
        }

        public PieceColor CurrentTurn { get; set; }

        public string ToFen()
        {
            return m_fen.Value;
        }

        public virtual void FromFen(string fen)
        {
            string[] fields = fen.Split(' ');
            string position = fields[0];
            string currentTurn = fields[1];
            string castling = fields[2];
            string enPassantTarget = fields[3];
            string halfMoveClock = fields[4];
            int index = 0;

            CurrentTurn = currentTurn == "b" ? PieceColor.Black : PieceColor.White;
            m_permanentCastlingRules.UpdateFromFen(castling);

            if (enPassantTarget != "-")
            {
                EnPassantTarget = (Location) Enum.Parse(typeof (Location), enPassantTarget.ToUpper());
            }

            m_fiftyMoveRule.ForceUpdate(Int32.Parse(halfMoveClock));

            SimpleBoard = new SimpleBoard(true);

            foreach (char c in position)
            {
                // A1 is square 0, H8 is square 63, but FEN reads from L->R, top to bottom
                int trueIndex = 8*(7 - index/8) + (index%8);
                if (Char.IsDigit(c))
                {
                    index += c - '0'; // Faster than Int32.Parse 
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

            m_fen.ForceUpdate(fen);
            m_checkCacheCurrentPlayer.OnCompleted();
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

            PreviousState = new BoardStateTransition
            {
                BoardBefore = new Board(this),
                MovedPiece = piece,
                Target = end
            };

            MovePiece(piece, end);

            PreviousState.BoardAfter = new Board(this);

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
            var x = StatusForBoard();
            return x == GameStatus.CheckmateBlackWins || x == GameStatus.CheckmateWhiteWins;
        }

        public IEnumerable<Location> FindPieces(PieceType pieceType)
        {
            return SimpleBoard.OccupiedSquares().Where(x => GetContents(x).Type == pieceType);
        }

        public bool Check => m_checkCacheCurrentPlayer.Value;

        public GameStatus StatusForBoard()
        {
            if (m_checkCacheCurrentPlayer.Value)
            {
                if (!ValidMovesExist())
                {
                    return CurrentTurn == PieceColor.White ? GameStatus.CheckmateBlackWins : GameStatus.CheckmateWhiteWins;
                }

                return GameStatus.Check;
            }
            else if (!ValidMovesExist())
            {
                return GameStatus.Stalemate;
            }
            else if (SimpleBoard.IsDraw())
            {
                return GameStatus.DrawInsufficientMaterial;
            }

            return GameStatus.None;
        }

        public void PromotePiece(string promotionTarget)
        {
            // Crappy, but we want to allow K (for Knight, when the UI asks for it) but not King.
            // We also want to work on the intial letters only, for when the parser reads a PGN file.
            if (promotionTarget.ToLower() == "king")
                throw new CannotPromoteException();

            var initialLetter = Char.ToUpper(promotionTarget[0]);
            var flagsForPromotedPiece = PieceType.None;

            // The trick here is that if white moved last (to put their pawn on the back rank) it's already been switched to Black's go,
            // so we need to promote to the opposite colour piece to make it all work.
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
                    PreviousState.Promotion = "N";
                    PromotePiece(PieceType.Knight | flagsForPromotedPiece);
                    break;
                default:
                    throw new CannotPromoteException();
            }

            PreviousState.BoardAfter = new Board(this);
            NotifyObservers();
        }

        public int FiftyMoveCounter => m_fiftyMoveRule.Value;

        public bool ValidateMoveForCheck(IPiece piece, Location newLocation)
        {
            var boardCopy = new Board(this);
            boardCopy.MovePiece(piece, newLocation);
            boardCopy.m_checkCacheOtherPlayer.OnCompleted(); // only need to tell one observer

            return !boardCopy.m_checkCacheOtherPlayer.Value;
        }

        public IPiece GetContents(Location loc)
        {
            return SimpleBoard.GetContents(loc);
        }

        public Location EnPassantTarget { get; private set; }

        public bool MayCastle(IPiece king, Side side)
        {
            if (king.Color == PieceColor.Black && side == Side.KingSide)
                return m_permanentCastlingRules.Value.HasFlag(CastlingOptions.BlackKingSide)
                       && m_transientCastlingRules.Value.HasFlag(CastlingOptions.BlackKingSide);
            if (king.Color == PieceColor.Black && side == Side.QueenSide)
                return m_permanentCastlingRules.Value.HasFlag(CastlingOptions.BlackQueenSide)
                       && m_transientCastlingRules.Value.HasFlag(CastlingOptions.BlackQueenSide);
            if (king.Color == PieceColor.White && side == Side.KingSide)
                return m_permanentCastlingRules.Value.HasFlag(CastlingOptions.WhiteKingSide)
                       && m_transientCastlingRules.Value.HasFlag(CastlingOptions.WhiteKingSide);
            if (king.Color == PieceColor.White && side == Side.QueenSide)
                return m_permanentCastlingRules.Value.HasFlag(CastlingOptions.WhiteQueenSide)
                       && m_transientCastlingRules.Value.HasFlag(CastlingOptions.WhiteQueenSide);

            throw new ArgumentException("Asked for impossible combination of castling");
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
            return (new CheckTester(colorOfKing, kingPosition, this)).Check();
        }

        private bool ValidMovesExist()
        {
            // Try the king first, because if we're doing a checkmate test, moving the king is the 
            // most likely way to get out of it.
            var king = (CurrentTurn == PieceColor.White ? PieceType.WhiteKing : PieceType.BlackKing);
            if (GetContents(FindPieces(king).First()).ValidMoves(this).Any())
                return true;

            // We know the king _can't_ move now, so don't call ValidMoves again if the piece is a king.
            var friends = SimpleBoard
                .Pieces(CurrentTurn)
                .OccupiedSquares()
                .Select(GetContents)
                .Where(p => !p.Type.IsOfType(PieceType.King))
                .ToArray();

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
                EnPassantTarget = Location.InvalidSquare;
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
                EnPassantTarget = Location.InvalidSquare;
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
                    EnPassantTarget = (new Square(originalLocation.X, originalLocation.Y + Math.Sign(verticalDistanceMoved)).Location);
                    return;
            }

            var horizontalDistanceMoved = newSquare.X - originalLocation.X;
            if (horizontalDistanceMoved != 0 && EnPassantTarget != Location.InvalidSquare)
                // The pawn has taken something but there was nothing on the square where it took, i.e. EP
            {
                var epSquare = new Square(EnPassantTarget);
                // Note sign. If our pawn moved forward (white) then the e.p. target is back down the board.
                SimpleBoard.RemovePiece(GetContents(new Square(epSquare.X, epSquare.Y - verticalDistanceMoved).Location));
            }

            EnPassantTarget = Location.InvalidSquare;
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

        public IDisposable Subscribe(IObserver<IBoardExtended> observer)
        {
            m_observers.Add(observer);
            return new Unsubscriber(this, observer);
        }

        public override int GetHashCode()
        {
            var key = FenCastleString().GetHashCode() ^ 397 * EnPassantTarget.GetHashCode() ^ 641 * (int)CurrentTurn;
            return SimpleBoard.OccupiedSquares().Aggregate(key, (current, p) => current ^ SimpleBoard.GetContents(p).GetHashCode());
        }

        sealed class Unsubscriber : IDisposable
        {
            private readonly Board m_board;
            private readonly IObserver<IBoardExtended> m_observer; 

            internal Unsubscriber(Board b, IObserver<IBoardExtended> observer)
            {
                m_board = b;
                m_observer = observer;
            }

            public void Dispose()
            {
                m_board.m_observers.Remove(m_observer);
            }
        }

        private void Dispose(bool isDisposing)
        {
            if (m_fen != null)
                m_fen.Dispose();
            if (m_transcriber != null)
                m_transcriber.Dispose();
            if (m_transientCastlingRules != null)
                m_transientCastlingRules.Dispose();
            if (m_fiftyMoveRule != null)
                m_fiftyMoveRule.Dispose();
            if (m_checkCacheCurrentPlayer != null)
                m_checkCacheCurrentPlayer.Dispose();
            if (m_checkCacheOtherPlayer != null)
                m_checkCacheOtherPlayer.Dispose();
            if((m_permanentCastlingRules != null))
                m_permanentCastlingRules.Dispose();
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
    }
}