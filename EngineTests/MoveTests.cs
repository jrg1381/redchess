using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using NUnit.Framework;
using RedChess.ChessCommon;
using RedChess.ChessCommon.Enumerations;
using Redchess.Engine;
using Redchess.Engine.Exceptions;
using Redchess.Engine.Pieces;
using Redchess.Engine.Pieces.Abstract;
using Redchess.Engine.Pieces.Black;
using Redchess.Engine.Pieces.White;

namespace EngineTests
{
    [TestFixture]
	internal class MoveTests : AbstractChessTest
	{
        [Test]
		public void SanityCheck()
		{
			Assert.AreEqual(0, m_emptyBoard.Pieces(PieceColor.White).OccupiedSquares().Count(), "Expected empty board to have no white pieces on it");
            Assert.AreEqual(0, m_emptyBoard.Pieces(PieceColor.Black).OccupiedSquares().Count(), "Expected empty board to have no black pieces on it");

			Assert.AreEqual(16, m_normalBoard.Pieces(PieceColor.White).OccupiedSquares().Count(), "Expected initial board to have 16 white pieces on it");
            Assert.AreEqual(16, m_normalBoard.Pieces(PieceColor.Black).OccupiedSquares().Count(), "Expected initial board to have 16 black pieces on it");
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

		[TestCase("8/8/8/8/8/8/P7/8 w KQkq -", Location.A2, new [] { Location.A3, Location.A4 })]
        [TestCase("8/8/8/8/8/P7/8/8 w KQkq -", Location.A3, new[] { Location.A4 })]
        [TestCase("8/8/8/8/3P4/3P4/8/8 w KQkq -", Location.D3, new Location[0])]
        [TestCase("rnbqkbnr/1ppppppp/8/8/p7/8/PPPPPPPP/RNBQKBNR w KQkq -", Location.A2, new [] { Location.A3 })]
        [TestCase("rnbqkbnr/1ppppppp/8/8/P7/8/PPPPPPP1/RNBQKBNR w KQkq -", Location.A2, new [] {Location.A3})]
		public void PawnMoves(string initialFen, Location pawnLocation, IEnumerable<Location> expectedReachableSquares)
		{
            m_emptyBoard.FromFen(initialFen);
            Assert.AreEqual(initialFen, m_emptyBoard.ToFen(), "Fen roundtrip failure");
            var reachableSquares = m_emptyBoard.GetContents(pawnLocation).ReachableSquares(m_emptyBoard);
            CollectionAssert.AreEquivalent(expectedReachableSquares, reachableSquares, "Set of reachable square for pawn was incorrect");
		}

		[Test]
		public void PawnCanAdvanceOrTake()
		{
            m_emptyBoard.FromFen("8/8/8/4p3/3P4/8/8/8 w KQkq -");
            var thePawn1 = m_emptyBoard.GetContents(Location.D4);
            CollectionAssert.AreEquivalent(new [] {Location.D5, Location.E5}, thePawn1.ReachableSquares(m_emptyBoard), "Set of reachable squares for pawn was incorrect");
		}

		[Test]
		public void PawnCannotAdvanceBlockedByEnemy()
		{
            m_emptyBoard.FromFen("8/8/8/3p4/3P4/8/8/8 w KQkq -");
            var thePawn1 = m_emptyBoard.GetContents(Location.D4);
            CollectionAssert.IsEmpty(thePawn1.ReachableSquares(m_emptyBoard), "Expected pawn to have no reachable squares");
		}

        [Test]
        public void PawnCannotAdvanceBlockedByFriend()
        {
            m_emptyBoard.FromFen("8/8/8/3P4/3P4/8/8/8 w KQkq -");
            var thePawn1 = m_emptyBoard.GetContents(Location.D4);
            CollectionAssert.IsEmpty(thePawn1.ReachableSquares(m_emptyBoard), "Expected pawn to have no reachable squares");
        }

        [Test]
        public void AwaitingPromotion()
        {
            m_emptyBoard.FromFen("8/P6k/8/8/8/8/1p5K/8 w KQkq -");

            Assert.False(m_emptyBoard.IsAwaitingPromotionDecision(), "Should not be waiting for promotion decision from white");
            m_emptyBoard.Move(Location.A7, Location.A8);
            Assert.True(m_emptyBoard.IsAwaitingPromotionDecision(), "Should be waiting for promotion decision from white");
            m_emptyBoard.PromotePiece("Queen");
            Assert.False(m_emptyBoard.IsAwaitingPromotionDecision(), "Waiting for promotion should be cancelled");
            m_emptyBoard.Move(Location.B2, Location.B1);
            Assert.True(m_emptyBoard.IsAwaitingPromotionDecision(), "Should be waiting for promotion decision from black");
            m_emptyBoard.PromotePiece("Queen");
            Assert.False(m_emptyBoard.IsAwaitingPromotionDecision(), "Waiting for promotion should be cancelled");
        }

	    [Test]
	    public void QueenMoves1()
	    {
            m_emptyBoard.FromFen("8/8/8/3p4/3q4/8/8/8 w KQkq -");
	        var theQueen = m_emptyBoard.GetContents(Location.D4);
	        var thePawn = m_emptyBoard.GetContents(Location.D5);
	        var blackPieces = m_emptyBoard.Pieces(PieceColor.Black);
	        Assert.AreEqual(2, blackPieces.OccupiedSquares().Count(), "Expected two black pieces on the board after adding a black queen and black pawn");
	        Assert.That(blackPieces.IsOccupied(Location.D4) && blackPieces.IsOccupied(Location.D5),
	            "Expected pieces to be on D4 and D5, but they were on " +
	            LocationListAsFriendlyString(m_emptyBoard.Pieces(PieceColor.Black).OccupiedSquares()));

            var reachableSquares = thePawn.ReachableSquares(m_emptyBoard).ToArray();
            var queenReachableSquares = theQueen.ReachableSquares(m_emptyBoard).ToArray();
	        Assert.AreEqual(0, reachableSquares.Length, "Expected pawn to have no reachable squares");
	        Assert.AreEqual(23, queenReachableSquares.Length, "Expected queen to have 23 reachable squares");
	        var expectedReachableSquares =
	            FriendlyStringToLocationList("A1,D1,G1,B2,D2,F2,C3,D3,E3,A4,B4,C4,E4,F4,G4,H4,C5,E5,B6,F6,A7,G7,H8");
	        CollectionAssert.AreEquivalent(expectedReachableSquares, queenReachableSquares,
	            "Queen reachable squares not as expected");
	    }

	    [TestCase("7k/6P1/8/8/8/8/8/8 b KQkq -")]
        [TestCase("3k4/2B5/8/8/8/8/8/8 b KQkq -")]
        [TestCase("3k4/5N2/8/8/8/8/8/8 b KQkq -")]
		public void BlackKingShouldBeInCheck(string fen)
		{
            m_emptyBoard.FromFen(fen);
            Assert.True(m_emptyBoard.KingInCheck(), "Black king should be in check");
		}

        [TestCase("7k/7P/8/8/8/8/8/K7 b KQkq -")]
        [TestCase("3k4/2b5/8/8/8/8/8/K7 b KQkq -")]
        [TestCase("3k3K/2r5/1B6/8/8/8/8/8 b KQkq -")]
        public void BlackKingShouldNotBeInCheck(string fen)
        {
            m_emptyBoard.FromFen(fen);
            Assert.False(m_emptyBoard.KingInCheck(), "Black king should not be in check");
        }

		[Test]
		public void PawnDoesNotAttackSquareInFrontOfIt()
		{
            m_emptyBoard.FromFen("3K3k/6P1/8/8/8/8/8/8 w KQkq -");
		    var theBlackKing = m_emptyBoard.GetContents(Location.H8);
            CollectionAssert.Contains(theBlackKing.ValidMoves(m_emptyBoard), Location.G8, "Pawn should not attack square in front of it");
		}

		[Test]
		public void PieceMovesFromOneSquareToAnother()
		{
            m_emptyBoard.FromFen("8/3k4/8/8/8/8/8/8 b KQ -");
			Assert.True(m_emptyBoard.Pieces(PieceColor.Black).IsOccupied(Location.D7), "Expected a piece on D7");
            Assert.False(m_emptyBoard.Pieces(PieceColor.Black).IsOccupied(Location.D8), "Didn't expect a piece on D8");
            Assert.AreEqual(1, m_emptyBoard.Pieces(PieceColor.Black).OccupiedSquares().Count(), "Expected only one black piece on the board");
            Assert.AreEqual(0, m_emptyBoard.Pieces(PieceColor.White).OccupiedSquares().Count(), "Expected no white pieces on the board");
		}

		[Test]
		public void CannotTakeOwnPiece()
		{
            m_emptyBoard.FromFen("3k4/3n4/8/8/8/8/8/8 b KQkq -");
            Assert.IsFalse(m_emptyBoard.Move(Location.D8, Location.D7), "Expected this move to fail - can't take own piece");
		}

        [Test]
        public void CannotPlayOutOfTurn()
        {
            m_emptyBoard.FromFen("7k/2Rn4/8/8/8/8/8/8 b KQkq -");
            Assert.IsFalse(m_emptyBoard.Move(Location.C7, Location.D7), "Cannot play out of turn");        
        }

		[Test]
		public void TakenPiecesAreRemovedFromTheBoard()
		{
            m_emptyBoard.FromFen("3N4/3k4/8/8/8/8/8/8 b KQ -");
            m_emptyBoard.Move(Location.D7, Location.D8);
            FenAssert.AreEqual("3k4/8/8/8/8/8/8/8 w KQ -", m_emptyBoard.ToFen(), "Expected knight to be taken");
		}

        [TestCase("7k/8/8/8/8/8/8/8 b - -", Location.H8, Location.B2)]
        [TestCase("8/7k/8/8/8/8/8/Q7 w KQkq -", Location.H7, Location.H8)]
        [TestCase("7k/8/8/8/8/8/8/Q7 w KQkq -", Location.H8, Location.G7)]
        [TestCase("7k/8/8/8/8/8/1n6/Q7 w KQkq -", Location.B2, Location.A4)]
        [TestCase("7k/8/8/8/n2p4/8/8/Q7 w KQkq -", Location.D4, Location.D5)]
        [TestCase("8/7k/8/8/8/8/8/8 b KQkq -", Location.H7, Location.H7)]
        public void IllegalMovesAreDisallowed(string fen, Location from, Location to)
        {
            m_emptyBoard.FromFen(fen);
            Assert.NotNull(m_emptyBoard.GetContents(from), "Expected a piece to be on the 'from' square");
            Assert.IsFalse(m_emptyBoard.Move(from, to), "Move should not be allowed");
            if (from != to)
            {
                Assert.AreNotEqual(m_emptyBoard.GetContents(to), m_emptyBoard.GetContents(from),
                    "Expected destination not to contain the moved piece");
            }
        }

		[Test]
		public void CannotMoveUntilOpponentMakesPromotionChoice()
		{
            m_emptyBoard.FromFen("8/8/8/8/8/6k1/3p4/Q5K1 b KQkq -");
            m_emptyBoard.Move(Location.D2, Location.D1); // but don't promote it
			// Try to move the queen, but this is not allowed because the previous player hasn't made a promotion choice yet
            Assert.Throws(typeof(InvalidMoveException), () => m_emptyBoard.Move(Location.A1, Location.A2));
		}

		[Test]
		public void KingHasReachableSquaresButNoneAreValidStalemate()
		{
            m_emptyBoard.FromFen("7k/2P2Q2/6R1/8/8/8/8/K7 b KQkq -");
		    var theKing = m_emptyBoard.GetContents(Location.H8);
            var kingReachableSquares = theKing.ReachableSquares(m_emptyBoard);
		    var expectedReachableSquares = new[] {Location.G7, Location.H7, Location.G8};
            CollectionAssert.AreEquivalent(expectedReachableSquares, kingReachableSquares, "King reachable squares not as expected");

            foreach (var loc in theKing.ReachableSquares(m_emptyBoard))
			{
                Assert.IsFalse(m_emptyBoard.Move(Location.H8, loc), "All king moves here should be disallowed");
			}

            CollectionAssert.IsEmpty(theKing.ValidMoves(m_emptyBoard), "The king should have no valid moves");
            Assert.True(m_emptyBoard.IsStalemate(), "Should be stalemate");
		}

        [Test]
		public void InitialMoves1()
		{
			var whitePawn = m_normalBoard.GetContents(Location.C2);
			Assert.IsInstanceOf(typeof(WhitePawn),whitePawn,"Expected a white pawn on C2");
		}

        [Test]
        public void PawnDoubleHopBlocked()
        {
            m_emptyBoard.FromFen("rnbqkbnr/pppppppp/8/8/8/Q7/PPPPPPPP/RNBQKBNR w KQkq -");
            var pawnReachableSquares = m_emptyBoard.GetContents(Location.A2).ReachableSquares(m_emptyBoard);
            CollectionAssert.IsEmpty(pawnReachableSquares, "Expected pawn to have no reachable squares");
        }

        [Test]
		public void PromotionByType()
		{
            m_emptyBoard.FromFen("k7/7P/8/8/8/8/8/4K3 w KQkq -");
			m_emptyBoard.Move(Location.H7, Location.H8);
			m_emptyBoard.PromotePiece("Queen");
			
			Assert.True(m_emptyBoard.KingInCheck(),"King should be in check after promotion of pawn");
		}

        [TestCase("Queen", Location.A8, true)]
        [TestCase("Rook", Location.A8, true)]
        [TestCase("Bishop", Location.F7, false)]
        [TestCase("Knight", Location.E7, false)]
		public void PromotionByString(string target, Location kingLocation, bool kingShouldBeInCheck)
		{
            m_emptyBoard.FromFen("k5r1/7P/8/8/8/8/8/4K3 w KQkq -");
			m_emptyBoard.Move(Location.H7, Location.G8);
            m_emptyBoard.PromotePiece(target);
			
			Assert.AreEqual(kingShouldBeInCheck, m_emptyBoard.KingInCheck(), "King should be in check after promotion of pawn");
		}

		[Test]
		public void BogusPromotionWhenNoPromotablePawn()
		{
			Assert.Throws(typeof(CannotPromoteException), () => m_normalBoard.PromotePiece("Rook"));
		}

        [TestCase("Pawn")]
        [TestCase("King")]
        public void BogusPromotionToRealPieceType(string pieceType)
        {
            Assert.Throws(typeof(CannotPromoteException), () => PromotionByString(pieceType, Location.A8, false));
        }

		[TestCase("7r/2k5/8/8/8/5q2/8/6K1/ w - -")]
        [TestCase("7r/2k5/2P5/8/8/5q2/8/6K1/ w - -")]
		public void StaleMateTest(string fen)
		{
			m_emptyBoard.FromFen(fen);		
			var king = m_emptyBoard.GetContents(Location.G1);
            Console.WriteLine(LocationListAsFriendlyString(king.ValidMoves(m_emptyBoard)));
			Assert.That(m_emptyBoard.IsStalemate(),"Should be stalemate - White has no moves");
		}

        [TestCase("7r/2k5/8/8/8/8/8/6K1/ w - -")]
        [TestCase("7r/2k5/2P5/8/8/8/8/6K1/ w - -")]
        public void NotStaleMateTest(string fen)
        {
            m_emptyBoard.FromFen(fen);          
            var king = m_emptyBoard.GetContents(Location.G1);
            Console.WriteLine(LocationListAsFriendlyString(king.ValidMoves(m_emptyBoard)));
            Assert.That(!m_emptyBoard.IsStalemate() && king.ValidMoves(m_emptyBoard).Count() == 3, "Should not be stalemate - White has 3 moves");
        }

        [TestCase("6qk/8/8/8/8/8/8/K7/ w - -")]
        public void NotDrawTest(string fen)
        {
            m_emptyBoard.FromFen(fen);
            Assert.That(!m_emptyBoard.IsDraw(), "Should not be a draw in this position");
        }

		[Test]
		public void FoolsMate()
		{
			// f3 e5
			// 2. g4 Qh4#

			m_normalBoard.Move(Location.F2, Location.F3);
            m_normalBoard.Move(Location.E7, Location.E5);
            m_normalBoard.Move(Location.G2, Location.G4);
            m_normalBoard.Move(Location.D8, Location.H4);

            Assert.True(m_normalBoard.IsCheckmate(false), "White king should be mated");
		}

        [Test]
		public void IsMateAndOnlyMate()
		{
			m_emptyBoard.FromFen("rn5k/7Q/2p3B1/pp1np3/4P3/8/PPP2PPP/RN2K2R b KQ -");
            // Both methods calls should return true
            bool isMate = m_emptyBoard.IsCheckmate(skipCheckTest: false) && m_emptyBoard.IsCheckmate(skipCheckTest: true);
            Assert.IsTrue(isMate, "Given position should be checkmate");
            Assert.IsFalse(m_emptyBoard.IsStalemate(), "Should not be stalemate in this position");
		}

        [Test]
        public void IsNotMate()
        {
            m_emptyBoard.FromFen("rn5k/8/2p3B1/pp1np3/4P3/8/PPP2PPP/RN2K2R b KQ -");
            // Shouldn't matter whether we test for check first or not, they should both be false
            bool isMate = m_emptyBoard.IsCheckmate(skipCheckTest: false) || m_emptyBoard.IsCheckmate(skipCheckTest: true);
            Assert.IsFalse(isMate, "Given position should not be checkmate because the king is not in check");
        }

		[Test]
		public void MovingRooksStopsCastling()
		{
			// Advance the pawns
			m_normalBoard.Move(Location.A2, Location.A3);
            m_normalBoard.Move(Location.A7, Location.A6);
            m_normalBoard.Move(Location.H2, Location.H3);
            m_normalBoard.Move(Location.H7, Location.H6);
			// Advance the rooks
            m_normalBoard.Move(Location.A1, Location.A2);
            m_normalBoard.Move(Location.H8, Location.H7);
            m_normalBoard.Move(Location.H1, Location.H2);
            m_normalBoard.Move(Location.A8, Location.A7);

            FenAssert.AreEqual("1nbqkbn1/rppppppr/p6p/8/8/P6P/RPPPPPPR/1NBQKBN1 w - -", m_normalBoard.ToFen(), "Expected no castling to be allowed after moving all the rooks");
		}

        [Test]
        public void EarlyExitFromCastlingUpdateFunction()
        {
            m_emptyBoard.FromFen("1nbqkbn1/rppppppr/p6p/8/8/P6P/RPPPPPPR/1NBQKBN1 w - -");
            m_emptyBoard.Move(Location.B2, Location.B3);
            // Exercises the early exit from Update in the castling rules class
        }

        [Test]
		public void EscapableMate()
		{
			// Mate isn't just that the King can't do anything, it's that NO pieces can do anything. In this case, the white bishop can take the rook.
            m_emptyBoard.FromFen("B6k/8/8/8/8/8/7r/4K2r w KQkq -");

            Assert.That(m_emptyBoard.KingInCheck(), "King should be in check");
            CollectionAssert.IsEmpty(m_emptyBoard.GetContents(Location.E1).ValidMoves(m_emptyBoard), "King should have nowhere to go");
            CollectionAssert.IsNotEmpty(m_emptyBoard.GetContents(Location.A8).ValidMoves(m_emptyBoard), "Bishop should have many moves");
            Assert.False(m_emptyBoard.IsCheckmate(false), "White king should not be mated");
		}

		[Test]
		public void MoveWhenNotYourTurn()
		{
			// Advance the pawn
			m_normalBoard.Move(Location.A2, Location.A3);
			// Advance the same pawn
            bool success = m_normalBoard.Move(Location.A3, Location.A4);
			Assert.False(success,"Should not be allowed to move twice in succession");
		}

		[Test]
		public void MoveNonExistentPiece()
		{
			// Advance nothing
            bool success = m_normalBoard.Move(Location.A3, Location.A4);
			Assert.False(success, "Should not be allowed to move a non-existent piece");
		}

		[Test]
		public void MovePawnBackwards()
		{
            Assert.False(m_normalBoard.Move(Location.A2, Location.A1), "Should not be allowed to move a pawn onto own pieces");
		}

        [TestCase("7k/8/8/8/8/8/8/K7/ w - -", "two kings")]
        [TestCase("7k/8/3b7/8/8/8/8/K7/ w - -", "king and bishop")]
        [TestCase("7k/8/8/8/8/6N1/8/K7/ w - -", "king and knight")]
		public void TestDraw(string fen, string description)
		{
            m_emptyBoard.FromFen(fen);
            Assert.True(m_emptyBoard.IsDraw(), "This position is a draw - " + description);
		}

        [TestCase("6nk/8/8/8/8/6N1/8/K7/ w - -", "king and two knights")]
        [TestCase("7k/8/8/8/8/6bn/8/K7/ w - -", "king, bishop and knight")]
        public void TestNotDraw(string fen, string description)
        {
            m_emptyBoard.FromFen(fen);
            Assert.False(m_emptyBoard.IsDraw(), "This position is not a draw - " + description);
        }
	}
}

