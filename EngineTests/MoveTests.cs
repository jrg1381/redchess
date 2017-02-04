using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;
using RedChess.ChessCommon.Enumerations;
using Redchess.Engine.Exceptions;
using Redchess.Engine.Pieces.White;

namespace Redchess.EngineTests
{
    [TestFixture]
	internal class MoveTests : AbstractChessTest
	{
        [Test]
		public void SanityCheck()
		{
			Assert.AreEqual(0, EmptyBoard.Pieces(PieceColor.White).OccupiedSquares().Count(), "Expected empty board to have no white pieces on it");
            Assert.AreEqual(0, EmptyBoard.Pieces(PieceColor.Black).OccupiedSquares().Count(), "Expected empty board to have no black pieces on it");

			Assert.AreEqual(16, NormalBoard.Pieces(PieceColor.White).OccupiedSquares().Count(), "Expected initial board to have 16 white pieces on it");
            Assert.AreEqual(16, NormalBoard.Pieces(PieceColor.Black).OccupiedSquares().Count(), "Expected initial board to have 16 black pieces on it");
		}

		private string LocationListAsFriendlyString(IEnumerable<Location> data)
		{
			var sb = new StringBuilder();
			foreach (Location loc in data)
			{
				sb.Append(loc);
				sb.Append(",");
			}
			if (sb.Length > 0)
				sb.Remove(sb.Length - 1, 1);
			return sb.ToString();
		}

		private IEnumerable<Location> FriendlyStringToLocationList(string friendlyString)
		{
			if (string.IsNullOrEmpty(friendlyString))
				yield break;

			var data = friendlyString.Split(new[] {','});
			foreach (var element in data)
			{
				yield return (Location) Enum.Parse(typeof (Location), element);
			}
		}

		[TestCase("k7/8/8/8/8/8/P7/7K w KQkq - 0", Location.A2, new [] { Location.A3, Location.A4 })]
        [TestCase("k7/8/8/8/8/P7/8/7K w KQkq - 0", Location.A3, new[] { Location.A4 })]
        [TestCase("k6K/8/8/8/3P4/3P4/8/8 w KQkq - 0", Location.D3, new Location[0])]
        [TestCase("rnbqkbnr/1ppppppp/8/8/p7/8/PPPPPPPP/RNBQKBNR w KQkq - 0", Location.A2, new [] { Location.A3 })]
        [TestCase("rnbqkbnr/1ppppppp/8/8/P7/8/PPPPPPP1/RNBQKBNR w KQkq - 0", Location.A2, new [] {Location.A3})]
		public void PawnMoves(string initialFen, Location pawnLocation, IEnumerable<Location> expectedReachableSquares)
		{
            EmptyBoard.FromFen(initialFen);
            Assert.AreEqual(initialFen, EmptyBoard.ToFen(), "Fen roundtrip failure");
            var reachableSquares = EmptyBoard.GetContents(pawnLocation).ReachableSquares(EmptyBoard);
            CollectionAssert.AreEquivalent(expectedReachableSquares, reachableSquares, "Set of reachable square for pawn was incorrect");
		}

		[Test]
		public void PawnCanAdvanceOrTake()
		{
            EmptyBoard.FromFen("8/8/8/4p3/3P4/8/8/k6K w KQkq - 0");
            var thePawn1 = EmptyBoard.GetContents(Location.D4);
            CollectionAssert.AreEquivalent(new [] {Location.D5, Location.E5}, thePawn1.ReachableSquares(EmptyBoard), "Set of reachable squares for pawn was incorrect");
		}

		[Test]
		public void PawnCannotAdvanceBlockedByEnemy()
		{
            EmptyBoard.FromFen("8/8/8/3p4/3P4/8/8/k6K w KQkq - 0");
            var thePawn1 = EmptyBoard.GetContents(Location.D4);
            CollectionAssert.IsEmpty(thePawn1.ReachableSquares(EmptyBoard), "Expected pawn to have no reachable squares");
		}

        [Test]
        public void PawnCannotAdvanceBlockedByFriend()
        {
            EmptyBoard.FromFen("8/8/8/3P4/3P4/8/8/k6K w KQkq - 0");
            var thePawn1 = EmptyBoard.GetContents(Location.D4);
            CollectionAssert.IsEmpty(thePawn1.ReachableSquares(EmptyBoard), "Expected pawn to have no reachable squares");
        }

        [Test]
        public void AwaitingPromotion()
        {
            EmptyBoard.FromFen("8/P6k/8/8/8/8/1p5K/8 w KQkq - 0");

            Assert.False(EmptyBoard.IsAwaitingPromotionDecision(), "Should not be waiting for promotion decision from white");
            EmptyBoard.Move(Location.A7, Location.A8);
            Assert.True(EmptyBoard.IsAwaitingPromotionDecision(), "Should be waiting for promotion decision from white");
            EmptyBoard.PromotePiece("Queen");
            Assert.False(EmptyBoard.IsAwaitingPromotionDecision(), "Waiting for promotion should be cancelled");
            EmptyBoard.Move(Location.B2, Location.B1);
            Assert.True(EmptyBoard.IsAwaitingPromotionDecision(), "Should be waiting for promotion decision from black");
            EmptyBoard.PromotePiece("Queen");
            Assert.False(EmptyBoard.IsAwaitingPromotionDecision(), "Waiting for promotion should be cancelled");
        }

	    [Test]
	    public void QueenMoves1()
	    {
            EmptyBoard.FromFen("k6K/8/8/3p4/3q4/8/8/8 w KQkq - 0");
	        var theQueen = EmptyBoard.GetContents(Location.D4);
	        var thePawn = EmptyBoard.GetContents(Location.D5);
	        var blackPieces = EmptyBoard.Pieces(PieceColor.Black);
	        Assert.AreEqual(3, blackPieces.OccupiedSquares().Count(), "Expected two black pieces on the board after adding a black queen and black pawn");
	        Assert.That(blackPieces.IsOccupied(Location.D4) && blackPieces.IsOccupied(Location.D5),
	            "Expected pieces to be on D4 and D5, but they were on " +
	            LocationListAsFriendlyString(EmptyBoard.Pieces(PieceColor.Black).OccupiedSquares()));

            var reachableSquares = thePawn.ReachableSquares(EmptyBoard).ToArray();
            var queenReachableSquares = theQueen.ReachableSquares(EmptyBoard).ToArray();
	        Assert.AreEqual(0, reachableSquares.Length, "Expected pawn to have no reachable squares");
	        Assert.AreEqual(23, queenReachableSquares.Length, "Expected queen to have 23 reachable squares");
	        var expectedReachableSquares =
	            FriendlyStringToLocationList("A1,D1,G1,B2,D2,F2,C3,D3,E3,A4,B4,C4,E4,F4,G4,H4,C5,E5,B6,F6,A7,G7,H8");
	        CollectionAssert.AreEquivalent(expectedReachableSquares, queenReachableSquares,
	            "Queen reachable squares not as expected");
	    }

	    [TestCase("7k/6P1/8/8/8/8/8/7K b - - 0")]
        [TestCase("3k4/2B5/8/8/8/8/8/7K b - - 0")]
        [TestCase("3k4/5N2/8/8/8/8/8/7K b - - 0")]
		public void BlackKingShouldBeInCheck(string fen)
		{
            EmptyBoard.FromFen(fen);
            Assert.True(EmptyBoard.StatusForBoard() == GameStatus.Check, "Black king should be in check");
		}

        [TestCase("7k/7P/8/8/8/8/8/K7 b - - 0")]
        [TestCase("3k4/2b5/8/8/8/8/8/K7 b - - 0")]
        [TestCase("3k3K/2r5/1B6/8/8/8/8/8 b - - 0")]
        public void BlackKingShouldNotBeInCheck(string fen)
        {
            EmptyBoard.FromFen(fen);
            Assert.False(EmptyBoard.StatusForBoard() == GameStatus.Check, "Black king should not be in check");
        }

		[Test]
		public void PawnDoesNotAttackSquareInFrontOfIt()
		{
            EmptyBoard.FromFen("3K3k/6P1/8/8/8/8/8/8 w KQkq - 0");
		    var theBlackKing = EmptyBoard.GetContents(Location.H8);
            CollectionAssert.Contains(theBlackKing.ValidMoves(EmptyBoard), Location.G8, "Pawn should not attack square in front of it");
		}

		[Test]
		public void PieceMovesFromOneSquareToAnother()
		{
            EmptyBoard.FromFen("8/3k4/8/8/8/8/8/7K b KQ - 0");
			Assert.True(EmptyBoard.Pieces(PieceColor.Black).IsOccupied(Location.D7), "Expected a piece on D7");
            Assert.False(EmptyBoard.Pieces(PieceColor.Black).IsOccupied(Location.D8), "Didn't expect a piece on D8");
            Assert.AreEqual(1, EmptyBoard.Pieces(PieceColor.Black).OccupiedSquares().Count(), "Expected only one black piece on the board");
            Assert.AreEqual(1, EmptyBoard.Pieces(PieceColor.White).OccupiedSquares().Count(), "Expected one white piece on the board");
		}

        [Test]
        public void CannotMoveIntoCheckByEnemyKing()
        {
            EmptyBoard.FromFen("k1K/8/8/8/8/8/8/8 b - - 0");
            var king = EmptyBoard.GetContents(Location.A8);
            Assert.False(EmptyBoard.ValidateMoveForCheck(king, Location.B8), "Black king cannot move into check by enemy king");
        }

		[Test]
		public void CannotTakeOwnPiece()
		{
            EmptyBoard.FromFen("3k4/3n4/8/8/8/8/8/7K b KQkq - 0");
            EmptyBoard.MoveExpectFailure(Location.D8, Location.D7);
		}

        [Test]
        public void CannotPlayOutOfTurn()
        {
            EmptyBoard.FromFen("7k/2Rn4/8/8/8/8/8/7K b KQkq - 0");
            EmptyBoard.MoveExpectFailure(Location.C7, Location.D7);
        }

		[Test]
		public void TakenPiecesAreRemovedFromTheBoard()
		{
            EmptyBoard.FromFen("3N4/3k4/8/8/8/8/8/7K b KQ - 0");
            EmptyBoard.Move(Location.D7, Location.D8);
            FenAssert.AreEqual("3k4/8/8/8/8/8/8/7K w KQ - 0", EmptyBoard.ToFen(), "Expected knight to be taken");
		}

        [TestCase("7k/8/8/8/8/8/8/7K b - - 0", Location.H8, Location.B2)]
        [TestCase("K7/7k/8/8/8/8/8/QK6 w KQkq - 0", Location.H7, Location.H8)]
        [TestCase("7k/8/8/8/8/8/8/QK6 w KQkq - 0", Location.H8, Location.G7)]
        [TestCase("7k/8/8/8/8/8/1n6/QK6 w KQkq - 0", Location.B2, Location.A4)]
        [TestCase("7k/8/8/8/n2p4/8/8/QK6 w KQkq - 0", Location.D4, Location.D5)]
        [TestCase("8/7k/8/8/8/8/8/7K b KQkq - 0", Location.H7, Location.H7)]
        public void IllegalMovesAreDisallowed(string fen, Location from, Location to)
        {
            EmptyBoard.FromFen(fen);
            Assert.NotNull(EmptyBoard.GetContents(from), "Expected a piece to be on the 'from' square");
            EmptyBoard.MoveExpectFailure(from, to);
            if (from != to)
            {
                Assert.AreNotEqual(EmptyBoard.GetContents(to), EmptyBoard.GetContents(from),
                    "Expected destination not to contain the moved piece");
            }
        }

		[Test]
		public void CannotMoveUntilOpponentMakesPromotionChoice()
		{
            EmptyBoard.FromFen("8/8/8/8/8/6k1/3p4/Q5K1 b KQkq - 0");
            EmptyBoard.Move(Location.D2, Location.D1); // but don't promote it
			// Try to move the queen, but this is not allowed because the previous player hasn't made a promotion choice yet
            Assert.Throws(typeof(InvalidMoveException), () => EmptyBoard.Move(Location.A1, Location.A2));
		}

		[Test]
		public void KingHasReachableSquaresButNoneAreValidStalemate()
		{
            EmptyBoard.FromFen("7k/2P2Q2/6R1/8/8/8/8/K7 b KQkq - 0");
		    var theKing = EmptyBoard.GetContents(Location.H8);
            var kingReachableSquares = theKing.ReachableSquares(EmptyBoard);
		    var expectedReachableSquares = new[] {Location.G7, Location.H7, Location.G8};
            CollectionAssert.AreEquivalent(expectedReachableSquares, kingReachableSquares, "King reachable squares not as expected");

            foreach (var loc in theKing.ReachableSquares(EmptyBoard))
            {
                EmptyBoard.MoveExpectFailure(Location.H8, loc);
            }

            CollectionAssert.IsEmpty(theKing.ValidMoves(EmptyBoard), "The king should have no valid moves");
            Assert.True(EmptyBoard.StatusForBoard() == GameStatus.Stalemate, "Should be stalemate");
		}

        [Test]
		public void InitialMoves1()
		{
			var whitePawn = NormalBoard.GetContents(Location.C2);
			Assert.IsInstanceOf(typeof(WhitePawn),whitePawn,"Expected a white pawn on C2");
		}

        [Test]
        public void PawnDoubleHopBlocked()
        {
            EmptyBoard.FromFen("rnbqkbnr/pppppppp/8/8/8/Q7/PPPPPPPP/RNBQKBNR w KQkq - 0");
            var pawnReachableSquares = EmptyBoard.GetContents(Location.A2).ReachableSquares(EmptyBoard);
            CollectionAssert.IsEmpty(pawnReachableSquares, "Expected pawn to have no reachable squares");
        }

        [Test]
		public void PromotionByType()
		{
            EmptyBoard.FromFen("k7/7P/8/8/8/8/8/4K3 w KQkq - 0");
			EmptyBoard.Move(Location.H7, Location.H8);
			EmptyBoard.PromotePiece("Queen");
			
			Assert.True(EmptyBoard.StatusForBoard() == GameStatus.Check, "King should be in check after promotion of pawn");
		}

        [TestCase("Queen", Location.A8, true)]
        [TestCase("Rook", Location.A8, true)]
        [TestCase("Bishop", Location.F7, false)]
        [TestCase("Knight", Location.E7, false)]
		public void PromotionByString(string target, Location kingLocation, bool kingShouldBeInCheck)
		{
            EmptyBoard.FromFen("k5r1/7P/8/8/8/8/8/4K3 w - - 0");
			EmptyBoard.Move(Location.H7, Location.G8);
            EmptyBoard.PromotePiece(target);
			
			Assert.AreEqual(kingShouldBeInCheck, EmptyBoard.StatusForBoard() == GameStatus.Check, "King should be in check after promotion of pawn");
		}

		[Test]
		public void BogusPromotionWhenNoPromotablePawn()
		{
		    NormalBoard.Move(Location.E2, Location.E4);
			Assert.Throws(typeof(CannotPromoteException), () => NormalBoard.PromotePiece("Rook"));
		}

        [TestCase("Pawn")]
        [TestCase("King")]
        public void BogusPromotionToRealPieceType(string pieceType)
        {
            Assert.Throws(typeof(CannotPromoteException), () => PromotionByString(pieceType, Location.A8, false));
        }

		[TestCase("7r/2k5/8/8/8/5q2/8/6K1/ w - - 0")]
        [TestCase("7r/2k5/2P5/8/8/5q2/8/6K1/ w - - 0")]
		public void StaleMateTest(string fen)
		{
			EmptyBoard.FromFen(fen);		
			var king = EmptyBoard.GetContents(Location.G1);
            Console.WriteLine(LocationListAsFriendlyString(king.ValidMoves(EmptyBoard)));
			Assert.That(EmptyBoard.StatusForBoard() == GameStatus.Stalemate,"Should be stalemate - White has no moves");
		}

        [TestCase("7r/2k5/8/8/8/8/8/6K1/ w - - 0")]
        [TestCase("7r/2k5/2P5/8/8/8/8/6K1/ w - - 0")]
        public void NotStaleMateTest(string fen)
        {
            EmptyBoard.FromFen(fen);          
            var king = EmptyBoard.GetContents(Location.G1);
            Console.WriteLine(LocationListAsFriendlyString(king.ValidMoves(EmptyBoard)));
            Assert.That(EmptyBoard.StatusForBoard() != GameStatus.Stalemate && king.ValidMoves(EmptyBoard).Count() == 3, "Should not be stalemate - White has 3 moves");
        }

        [TestCase("6qk/8/8/8/8/8/8/K7/ w - - 0")]
        public void NotDrawTest(string fen)
        {
            EmptyBoard.FromFen(fen);
            Assert.That(EmptyBoard.StatusForBoard() != GameStatus.DrawInsufficientMaterial, "Should not be a draw in this position");
        }

		[Test]
		public void FoolsMate()
		{
			// f3 e5
			// 2. g4 Qh4#

			NormalBoard.Move(Location.F2, Location.F3);
            NormalBoard.Move(Location.E7, Location.E5);
            NormalBoard.Move(Location.G2, Location.G4);
            NormalBoard.Move(Location.D8, Location.H4);

            Assert.True(NormalBoard.StatusForBoard() == GameStatus.CheckmateBlackWins, "White king should be mated");
		}

        [Test]
        public void IsNotMate()
        {
            EmptyBoard.FromFen("rn5k/8/2p3B1/pp1np3/4P3/8/PPP2PPP/RN2K2R b KQ - 0");
            // Shouldn't matter whether we test for check first or not, they should both be false
            bool isCheckOrMate = EmptyBoard.Check || EmptyBoard.StatusForBoard() == GameStatus.CheckmateBlackWins ||
                          EmptyBoard.StatusForBoard() == GameStatus.CheckmateWhiteWins;
            Assert.IsFalse(isCheckOrMate, "Given position should not be checkmate because the king is not in check");
        }

		[Test]
		public void MovingRooksStopsCastling()
		{
			// Advance the pawns
			NormalBoard.Move(Location.A2, Location.A3);
            NormalBoard.Move(Location.A7, Location.A6);
            NormalBoard.Move(Location.H2, Location.H3);
            NormalBoard.Move(Location.H7, Location.H6);
			// Advance the rooks
            NormalBoard.Move(Location.A1, Location.A2);
            NormalBoard.Move(Location.H8, Location.H7);
            NormalBoard.Move(Location.H1, Location.H2);
            NormalBoard.Move(Location.A8, Location.A7);

            FenAssert.AreEqual("1nbqkbn1/rppppppr/p6p/8/8/P6P/RPPPPPPR/1NBQKBN1 w - - 4", NormalBoard.ToFen(), "Expected no castling to be allowed after moving all the rooks");
		}

        [Test]
        public void EarlyExitFromCastlingUpdateFunction()
        {
            EmptyBoard.FromFen("1nbqkbn1/rppppppr/p6p/8/8/P6P/RPPPPPPR/1NBQKBN1 w - - 0");
            EmptyBoard.Move(Location.B2, Location.B3);
            // Exercises the early exit from Update in the castling rules class
        }

        [Test]
		public void EscapableMate()
		{
			// Mate isn't just that the King can't do anything, it's that NO pieces can do anything. In this case, the white bishop can take the rook.
            EmptyBoard.FromFen("B6k/8/8/8/8/8/7r/4K2r w KQkq - 0");

            Assert.That(EmptyBoard.StatusForBoard() == GameStatus.Check, "King should be in check");
            CollectionAssert.IsEmpty(EmptyBoard.GetContents(Location.E1).ValidMoves(EmptyBoard), "King should have nowhere to go");
            CollectionAssert.IsNotEmpty(EmptyBoard.GetContents(Location.A8).ValidMoves(EmptyBoard), "Bishop should have many moves");
            Assert.False(EmptyBoard.StatusForBoard() == GameStatus.CheckmateBlackWins, "White king should not be mated");
		}

		[Test]
		public void MoveWhenNotYourTurn()
		{
			// Advance the pawn
			NormalBoard.Move(Location.A2, Location.A3);
			// Advance the same pawn
            NormalBoard.MoveExpectFailure(Location.A3, Location.A4);
		}

		[Test]
		public void MoveNonExistentPiece()
		{
			// Advance nothing
            NormalBoard.MoveExpectFailure(Location.A3, Location.A4);
		}

		[Test]
		public void MovePawnBackwardsOntoOwnPieces()
		{
		    NormalBoard.MoveExpectFailure(Location.A2, Location.A1);
		}

        [TestCase("7k/8/8/8/8/8/8/K7/ w - - 0", "two kings")]
        [TestCase("7k/8/3b7/8/8/8/8/K7/ w - - 0", "king and bishop")]
        [TestCase("7k/8/8/8/8/6N1/8/K7/ w - - 0", "king and knight")]
		public void TestDraw(string fen, string description)
		{
            EmptyBoard.FromFen(fen);
            Assert.True(EmptyBoard.StatusForBoard() == GameStatus.DrawInsufficientMaterial, "This position is a draw - " + description);
		}

        [TestCase("6nk/8/8/8/8/6N1/8/K7/ w - - 0", "king and two knights")]
        [TestCase("7k/8/8/8/8/6bn/8/K7/ w - - 0", "king, bishop and knight")]
        public void TestNotDraw(string fen, string description)
        {
            EmptyBoard.FromFen(fen);
            Assert.True(EmptyBoard.StatusForBoard() != GameStatus.DrawInsufficientMaterial, "This position is not a draw - " + description);
        }
	}
}

