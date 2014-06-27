using System;
using System.Collections.Generic;
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
            m_betterBoardEmpty.FromFen(initialFen);
            Assert.AreEqual(initialFen, m_betterBoardEmpty.ToFen(), "Fen roundtrip failure");
            var reachableSquares = m_betterBoardEmpty.GetContents(pawnLocation).ReachableSquares(m_betterBoardEmpty);
            CollectionAssert.AreEquivalent(expectedReachableSquares, reachableSquares, "Set of reachable square for pawn was incorrect");
		}

		[Test]
		public void PawnCanAdvanceOrTake()
		{
			var thePawn1 = new WhitePawn(Location.D4);
			var thePawn2 = new BlackPawn(Location.E5);
			m_emptyBoard.AddPiece(thePawn1);
			m_emptyBoard.AddPiece(thePawn2);
            CollectionAssert.AreEquivalent(new [] {Location.D5, Location.E5}, thePawn1.ReachableSquares(m_betterBoardEmpty), "Set of reachable squares for pawn was incorrect");
		}

		[Test]
		public void PawnCannotAdvanceBlockedByEnemy()
		{
			var thePawn1 = new WhitePawn(Location.D4);
			var thePawn2 = new BlackPawn(Location.D5);
			m_emptyBoard.AddPiece(thePawn1);
			m_emptyBoard.AddPiece(thePawn2);
            CollectionAssert.IsEmpty(thePawn1.ReachableSquares(m_emptyBoard), "Expected pawn to have no reachable squares");
		}

        [Test]
        public void PawnCannotAdvanceBlockedByFriend()
        {
            var thePawn1 = new WhitePawn(Location.D4);
            var thePawn2 = new WhitePawn(Location.D5);
            m_emptyBoard.AddPiece(thePawn1);
            m_emptyBoard.AddPiece(thePawn2);
            CollectionAssert.IsEmpty(thePawn1.ReachableSquares(m_emptyBoard), "Expected pawn to have no reachable squares");
        }

        [Test]
        public void AwaitingPromotion()
        {
            m_betterBoardEmpty.FromFen("8/P6k/8/8/8/8/1p5K/8 w KQkq -");

            Assert.False(m_betterBoardEmpty.IsAwaitingPromotionDecision(), "Should not be waiting for promotion decision from white");
            m_betterBoardEmpty.Move(Location.A7, Location.A8);
            Assert.True(m_betterBoardEmpty.IsAwaitingPromotionDecision(), "Should be waiting for promotion decision from white");
            m_betterBoardEmpty.PromotePiece("Queen");
            Assert.False(m_betterBoardEmpty.IsAwaitingPromotionDecision(), "Waiting for promotion should be cancelled");
            m_betterBoardEmpty.Move(Location.B2, Location.B1);
            Assert.True(m_betterBoardEmpty.IsAwaitingPromotionDecision(), "Should be waiting for promotion decision from black");
            m_betterBoardEmpty.PromotePiece("Queen");
            Assert.False(m_betterBoardEmpty.IsAwaitingPromotionDecision(), "Waiting for promotion should be cancelled");
        }

	    [Test]
	    public void QueenMoves1()
	    {
	        var theQueen = new BlackQueen(Location.D4);
	        var thePawn = new BlackPawn(Location.D5);
	        m_emptyBoard.AddPiece(theQueen);
	        m_emptyBoard.AddPiece(thePawn);

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
            m_betterBoardEmpty.FromFen(fen);
            Assert.True(m_betterBoardEmpty.KingInCheck(), "Black king should be in check");
		}

        [TestCase("7k/7P/8/8/8/8/8/K7 b KQkq -")]
        [TestCase("3k4/2b5/8/8/8/8/8/K7 b KQkq -")]
        [TestCase("3k3K/2r5/1B6/8/8/8/8/8 b KQkq -")]
        public void BlackKingShouldNotBeInCheck(string fen)
        {
            m_betterBoardEmpty.FromFen(fen);
            Assert.False(m_betterBoardEmpty.KingInCheck(), "Black king should not be in check");
        }

		[Test]
		public void PawnDoesNotAttackSquareInFrontOfIt()
		{
            m_betterBoardEmpty.FromFen("3K3k/6P1/8/8/8/8/8/8 w KQkq -");
		    var theBlackKing = m_betterBoardEmpty.GetContents(Location.H8);
            CollectionAssert.Contains(theBlackKing.ValidMoves(m_betterBoardEmpty), Location.G8, "Pawn should not attack square in front of it");
		}

		[Test]
		public void MoveTest1()
		{
			var theKing = new BlackKing(Location.D8);
			m_emptyBoard.AddPiece(theKing);
			m_emptyBoard.CurrentTurn = PieceColor.Black;
			m_emptyBoard.MovePiece(theKing,Location.D7);
			Assert.True(m_emptyBoard.Pieces(PieceColor.Black).IsOccupied(Location.D7), "Expected a piece on D7");
			Assert.False(m_emptyBoard.Pieces(PieceColor.Black).IsOccupied(Location.D8), "Didn't expect a piece on D8");
			Assert.AreEqual(1, m_emptyBoard.Pieces(PieceColor.Black).OccupiedSquares().Count(),"Expected only one black piece on the board");
			Assert.AreEqual(0, m_emptyBoard.Pieces(PieceColor.White).OccupiedSquares().Count(), "Expected no white pieces on the board");
		}

		[Test]
		public void MoveTest2()
		{
			var theKing = new BlackKing(Location.D8);
			var theKnight = new BlackKnight(Location.D7);
			m_emptyBoard.AddPiece(theKing);
			m_emptyBoard.AddPiece(theKnight);
		    Assert.IsFalse(m_emptyBoard.Move(Location.D8, Location.D7), "Expected this move to fail - can't take own piece");
		}

		[Test]
		public void MoveTest3()
		{
			var theKing = new BlackKing(Location.D8);
			var theKnight = new WhiteKnight(Location.D7);
			m_emptyBoard.AddPiece(theKing);
			m_emptyBoard.AddPiece(theKnight);
			m_emptyBoard.CurrentTurn = PieceColor.Black;
			m_emptyBoard.MovePiece(theKing, Location.D7);
			Assert.AreEqual(0, m_emptyBoard.Pieces(PieceColor.White).OccupiedSquares().Count(), "Expected no white pieces on the board");
		}

		[Test]
		public void MoveTest4()
		{
			var theKing = new BlackKing(Location.D8);
			m_emptyBoard.CurrentTurn = PieceColor.Black;
            Assert.IsFalse(m_emptyBoard.Move(Location.D8, Location.A1), "Move should not be allowed");
		}

		[Test]
		public void MoveTest5()
		{
			var theKing = new BlackKing(Location.H7);
			var theQueen = new WhiteQueen(Location.A1);
			m_emptyBoard.AddPiece(theKing);
			m_emptyBoard.AddPiece(theQueen);
            Assert.IsFalse(m_emptyBoard.Move(Location.H7, Location.H8), "Move should not be allowed");
		}

		[Test]
		public void MoveTest6()
		{
			var theKing = new BlackKing(Location.H8);
			var theQueen = new WhiteQueen(Location.A1);
			m_emptyBoard.AddPiece(theKing);
			m_emptyBoard.AddPiece(theQueen);
            Assert.IsFalse(m_emptyBoard.Move(Location.H8, Location.G7), "Move should not be allowed");
		}

		[Test]
		public void MoveTest7()
		{
			var theKing = new BlackKing(Location.H8);
			var theQueen = new WhiteQueen(Location.A1);
			var theKnight = new BlackKnight(Location.B2);
			m_emptyBoard.AddPiece(theKing);
			m_emptyBoard.AddPiece(theQueen);
			m_emptyBoard.AddPiece(theKnight);
            Assert.IsFalse(m_emptyBoard.Move(Location.B2, Location.A4), "Move should not be allowed");
		}

		[Test]
		public void MoveTest8()
		{
			var theKing = new BlackKing(Location.H8);
			var theQueen = new WhiteQueen(Location.A1);
			var theKnight = new BlackKnight(Location.B2);
			var thePawn = new BlackPawn(Location.D4);
			m_emptyBoard.AddPiece(theKing);
			m_emptyBoard.AddPiece(theQueen);
			m_emptyBoard.AddPiece(theKnight);
			m_emptyBoard.AddPiece(thePawn);
			m_emptyBoard.CurrentTurn = PieceColor.Black;
			m_emptyBoard.MovePiece(theKnight, Location.A4);
            Assert.IsFalse(m_emptyBoard.Move(Location.D4, Location.D5), "Move should not be allowed");
		}

		[Test]
		public void MoveTest9()
		{
			var theWhiteKing = new WhiteKing(Location.G1);
			var theBlackKing = new BlackKing(Location.G3);
			var theQueen = new WhiteQueen(Location.A1);
			var thePawn = new BlackPawn(Location.D2);
			m_emptyBoard.AddPiece(theQueen);
			m_emptyBoard.AddPiece(thePawn);
			m_emptyBoard.AddPiece(theWhiteKing);
			m_emptyBoard.AddPiece(theBlackKing);
			m_emptyBoard.CurrentTurn = PieceColor.Black;
			m_emptyBoard.MovePiece(thePawn, Location.D1); // but don't promote it
			// Try to move the queen, but this is not allowed because the previous player hasn't made a promotion choice yet
			Assert.Throws(typeof(InvalidMoveException), () => m_emptyBoard.MovePiece(theQueen, Location.A2));
		}

	    [Test]
	    public void MoveTest10()
	    {
            var theKing = new BlackKing(Location.H7);
            m_emptyBoard.AddPiece(theKing);
	        bool moveOk = m_emptyBoard.Move(Location.H7, Location.H7);
            Assert.False(moveOk, "Should not be able to move a piece to the square it's already on");
	    }

		[Test]
		public void NoValidMoves1()
		{
			m_emptyBoard.CurrentTurn = PieceColor.Black;
			m_emptyBoard.AddPiece(new WhiteKing(Location.A1));
			var theKing = new BlackKing(Location.H8);
			var theQueen = new WhiteQueen(Location.F7);
			var theRook = new WhiteRook(Location.G6);
			var thePawn = new WhitePawn(Location.C7);
			m_emptyBoard.AddPiece(theKing);
			m_emptyBoard.AddPiece(theQueen);
			m_emptyBoard.AddPiece(theRook);
			m_emptyBoard.AddPiece(thePawn);
            var kingReachableSquares = theKing.ReachableSquares(m_emptyBoard);
			var expectedReachableSquares = FriendlyStringToLocationList("G7,H7,G8");
            CollectionAssert.AreEquivalent(expectedReachableSquares, kingReachableSquares, "King reachable squares not as expected");
			
            Console.WriteLine(m_emptyBoard);

            foreach (var loc in theKing.ReachableSquares(m_emptyBoard))
			{
				var copyOfLoc = loc;
                Assert.IsFalse(m_emptyBoard.Move(Location.H8, copyOfLoc), "All king moves here should be disallowed");
			}
            Assert.AreEqual(0, theKing.ValidMoves(m_emptyBoard).Count(), "The king should have no valid moves");
            Assert.True(m_emptyBoard.IsStalemate(), "Should be stalemate");
		}

		[Test]
		public void CastlingTest1()
		{
			var theKing = new WhiteKing(Location.E1);
			var theRook = new WhiteRook(Location.H1);
			m_emptyBoard.AddPiece(theKing);
			m_emptyBoard.AddPiece(theRook);
			Assert.True(m_emptyBoard.MayCastle(theKing, Side.KingSide), "Untouched king and rook should be allowed to castle");
		}

		[Test]
		public void CastlingTest2()
		{
			var theKing = new WhiteKing(Location.E1);
			var theRook = new WhiteRook(Location.H1);
			var theQueen = new BlackQueen(Location.E4);
			m_emptyBoard.AddPiece(theKing);
			m_emptyBoard.AddPiece(theRook);
			m_emptyBoard.AddPiece(theQueen);
			Assert.False(m_emptyBoard.MayCastle(theKing, Side.KingSide),"The king is in check right now! No castling allowed.");
		}

		[Test]
		public void CastlingTest3()
		{
			// The king would pass through check! No castling allowed.
			var theKing = new WhiteKing(Location.E1);
			var theRook = new WhiteRook(Location.H1);
			var theQueen = new BlackQueen(Location.F4);
			m_emptyBoard.AddPiece(theKing);
			m_emptyBoard.AddPiece(theRook);
			m_emptyBoard.AddPiece(theQueen);
			Assert.False(m_emptyBoard.MayCastle(theKing, Side.KingSide), "The king would pass through check! No castling allowed.");
		}

		[Test]
		public void CastlingTest4()
		{
			var theKing = new WhiteKing(Location.E1);
			var theRook = new WhiteRook(Location.H1);
			var theQueen = new BlackQueen(Location.G4);
			m_emptyBoard.AddPiece(theKing);
			m_emptyBoard.AddPiece(theRook);
			m_emptyBoard.AddPiece(theQueen);
			Assert.False(m_emptyBoard.MayCastle(theKing, Side.KingSide), "The king would finish in check! No castling allowed.");
		}

		[Test]
		public void CastlingTest5()
		{
			m_emptyBoard.CurrentTurn = PieceColor.White;
			m_emptyBoard.AddPiece(new BlackKing(Location.H8));
			var theKing = new WhiteKing(Location.E1);
			var theRook = new WhiteRook(Location.H1);
			m_emptyBoard.AddPiece(theKing);
			m_emptyBoard.AddPiece(theRook);
			m_emptyBoard.MovePiece(theKing, Location.E2);
			// Let white move twice
			m_emptyBoard.CurrentTurn = PieceColor.White;
			m_emptyBoard.MovePiece(theKing, Location.E1);
			Assert.False(m_emptyBoard.MayCastle(theKing, Side.KingSide),"The king has been moved. No castling allowed.");
		}

		[Test]
		public void CastlingTest6()
		{
			var theKing = new WhiteKing(Location.E1);
			var theBishop = new BlackBishop(Location.F1);
			var theRook = new WhiteRook(Location.H1);
			m_emptyBoard.AddPiece(theKing);
			m_emptyBoard.AddPiece(theRook);
			m_emptyBoard.AddPiece(theBishop);
			Assert.False(m_emptyBoard.MayCastle(theKing, Side.KingSide), "The king is blocked by another piece. No castling.");
		}

		[Test]
		public void CastlingTest7()
		{
			m_emptyBoard.AddPiece(new BlackKing(Location.B5));
			var theKing = new WhiteKing(Location.E1);
			var theBishop = new BlackBishop(Location.A8);
			var theRook1 = new WhiteRook(Location.H1);
			var theRook2 = new WhiteRook(Location.A1);
			m_emptyBoard.AddPiece(theKing);
			m_emptyBoard.AddPiece(theRook1);
			m_emptyBoard.AddPiece(theRook2);
			m_emptyBoard.AddPiece(theBishop);
			m_emptyBoard.CurrentTurn = PieceColor.Black;
			m_emptyBoard.MovePiece(theBishop, theRook1.Position.Location);
			Assert.False(m_emptyBoard.MayCastle(theKing, Side.KingSide), "The rook was taken by the bishop");
			Assert.True(m_emptyBoard.MayCastle(theKing, Side.QueenSide), "Should still be ok to castle queen side");
		}

		[Test]
		public void CastlingTest8()
		{
			m_emptyBoard.AddPiece(new BlackKing(Location.H8));
			var theKing = new WhiteKing(Location.E1);
			var theRook = new WhiteRook(Location.H1);
			m_emptyBoard.AddPiece(theKing);
			m_emptyBoard.AddPiece(theRook);
			Console.WriteLine(m_emptyBoard);
			Assert.True(m_emptyBoard.MayCastle(theKing, Side.KingSide), "Untouched white king and rook should be allowed to castle");
			m_emptyBoard.MovePiece(theKing,Location.G1);
			Console.WriteLine(m_emptyBoard);
		}

		[Test]
		public void CastlingTest8b()
		{
			m_emptyBoard.CurrentTurn = PieceColor.Black;
			m_emptyBoard.AddPiece(new WhiteKing(Location.H1));
			var theKing = new BlackKing(Location.E8);
			var theRook = new BlackRook(Location.H8);
			m_emptyBoard.AddPiece(theKing);
			m_emptyBoard.AddPiece(theRook);
			Console.WriteLine(m_emptyBoard);
			Assert.True(m_emptyBoard.MayCastle(theKing, Side.KingSide), "Untouched black king and rook should be allowed to castle");
			m_emptyBoard.MovePiece(theKing, Location.G8);
			Console.WriteLine(m_emptyBoard);
		}

		[Test]
		public void CastlingTest9()
		{
			m_emptyBoard.AddPiece(new BlackKing(Location.H8));
			var theKing = new WhiteKing(Location.E1);
			var theRook = new WhiteRook(Location.A1);
			m_emptyBoard.AddPiece(theKing);
			m_emptyBoard.AddPiece(theRook);
			Console.WriteLine(m_emptyBoard);
			Assert.True(m_emptyBoard.MayCastle(theKing, Side.QueenSide), "Untouched king and rook should be allowed to castle");
			m_emptyBoard.MovePiece(theKing, Location.C1);
			Assert.False(m_emptyBoard.MayCastle(theKing, Side.QueenSide), "Castling has occurred already, not allowed again");
			Console.WriteLine(m_emptyBoard);
		}

		[Test]
		public void CastlingTest10()
		{
			m_emptyBoard.AddPiece(new WhiteKing(Location.A1));
			var theKing = new BlackKing(Location.E8);
			var theRook = new BlackRook(Location.A8);
			m_emptyBoard.CurrentTurn = PieceColor.Black;
			m_emptyBoard.AddPiece(theKing);
			m_emptyBoard.AddPiece(theRook);
			Console.WriteLine(m_emptyBoard);
			Assert.True(m_emptyBoard.MayCastle(theKing, Side.QueenSide), "Untouched king and rook should be allowed to castle");
			m_emptyBoard.MovePiece(theKing, Location.C8);
			Assert.False(m_emptyBoard.MayCastle(theKing, Side.QueenSide), "Castling has occurred already, not allowed again");
			Console.WriteLine(m_emptyBoard);
		}

		[Test]
		public void InitialMoves1()
		{
			var whitePawn = m_normalBoard.GetContents(Location.C2);
			Assert.IsInstanceOf(typeof(WhitePawn),whitePawn,"Expected a white pawn on C2");
		}

		[Test]
		public void InitialMoves2()
		{
			var whitePawn = m_normalBoard.GetContents(Location.C3);
			Assert.IsNull(whitePawn, "Expected nothing on C3");
		}

		[Test]
		public void EnPassant1()
		{
			var pawn1 = m_normalBoard.GetContents(Location.D2);
			m_normalBoard.MovePiece(pawn1,Location.D4);
			Assert.AreEqual(Location.D3, m_normalBoard.EnPassantTarget,"D3 should be an e.p. target");
			Console.WriteLine(m_normalBoard);
			var pawn2 = m_normalBoard.GetContents(Location.D7);
			m_normalBoard.MovePiece(pawn2, Location.D5);
			Assert.AreEqual(Location.D6, m_normalBoard.EnPassantTarget, "D3 should be an e.p. target");
			Console.WriteLine(m_normalBoard);
		}

		[Test]
		public void EnPassant2()
		{
			// Black takes via EP
			m_normalBoard.MovePiece(m_normalBoard.GetContents(Location.A2), Location.A3);
			Console.WriteLine(m_normalBoard);
			m_normalBoard.MovePiece(m_normalBoard.GetContents(Location.E7), Location.E5);
			Console.WriteLine(m_normalBoard);
			m_normalBoard.MovePiece(m_normalBoard.GetContents(Location.A3), Location.A4);
			Console.WriteLine(m_normalBoard);
			m_normalBoard.MovePiece(m_normalBoard.GetContents(Location.E5), Location.E4);
			Console.WriteLine(m_normalBoard); 
			m_normalBoard.MovePiece(m_normalBoard.GetContents(Location.D2), Location.D4);
			Console.WriteLine(m_normalBoard);
			m_normalBoard.MovePiece(m_normalBoard.GetContents(Location.E4), Location.D3);
			Console.WriteLine(m_normalBoard);
			Assert.AreEqual("rnbqkbnr/pppp1ppp/8/8/P7/3p4/1PP1PPPP/RNBQKBNR w KQkq -", m_normalBoard.ToFen(), "Board does not look as expected after black EP capture");
		}

		[Test]
		public void EnPassant3()
		{
			// White takes via EP
			m_normalBoard.MovePiece(m_normalBoard.GetContents(Location.B2), Location.B4);
			Console.WriteLine(m_normalBoard);
			m_normalBoard.MovePiece(m_normalBoard.GetContents(Location.E7), Location.E5);
			Console.WriteLine(m_normalBoard);
			m_normalBoard.MovePiece(m_normalBoard.GetContents(Location.B4), Location.B5);
			Console.WriteLine(m_normalBoard);
			m_normalBoard.MovePiece(m_normalBoard.GetContents(Location.A7), Location.A5);
			Console.WriteLine(m_normalBoard);
			m_normalBoard.MovePiece(m_normalBoard.GetContents(Location.B5), Location.A6);
			Console.WriteLine(m_normalBoard);
			Assert.AreEqual("rnbqkbnr/1ppp1ppp/P7/4p3/8/8/P1PPPPPP/RNBQKBNR b KQkq -", m_normalBoard.ToFen(), "Board does not look as expected after white EP capture");
		}

        [Test]
        public void PawnDoubleHopBlocked()
        {
            m_normalBoard.AddPiece(PieceFactory.CreatePiece(PieceType.WhiteQueen,Location.A3));
            var pawnReachableSquares = m_normalBoard.GetContents(Location.A2).ReachableSquares(m_normalBoard);
            Assert.AreEqual(0, pawnReachableSquares.Count(), "Expected pawn to have no reachable squares");
        }

        [Test]
        public void EnPassantInFenWhiteTarget()
        {
            // Black takes via EP
            m_normalBoard.MovePiece(m_normalBoard.GetContents(Location.A2), Location.A3);
            Console.WriteLine(m_normalBoard);
            m_normalBoard.MovePiece(m_normalBoard.GetContents(Location.E7), Location.E5);
            Console.WriteLine(m_normalBoard);
            m_normalBoard.MovePiece(m_normalBoard.GetContents(Location.A3), Location.A4);
            Console.WriteLine(m_normalBoard);
            m_normalBoard.MovePiece(m_normalBoard.GetContents(Location.E5), Location.E4);
            Console.WriteLine(m_normalBoard);
            m_normalBoard.MovePiece(m_normalBoard.GetContents(Location.D2), Location.D4);
            Console.WriteLine(m_normalBoard);
            Assert.AreEqual("rnbqkbnr/pppp1ppp/8/8/P2Pp3/8/1PP1PPPP/RNBQKBNR b KQkq D3", m_normalBoard.ToFen(), "Board does not look as expected with pending EP capture");
        }

	    [Test]
	    public void EnPassantDoesNotBlockTaking()
	    {
            // This was a problem which happened when the e.p. bitmaps were included in the calculation of the occupied squares, causing the invisible ep target to block other pieces.
            m_emptyBoard.FromFen("rnbqkbnr/ppp2ppp/3p4/4P3/8/6PB/PPPPPP1P/RNBQK1NR w KQkq - E6");
	        var theBishop = m_emptyBoard.GetContents(Location.H3);
            Assert.That(theBishop.Type.IsOfType(PieceType.WhiteBishop), "The white bishop is not a white bishop! " + theBishop.Type);
            CollectionAssert.AreEquivalent(FriendlyStringToLocationList("G2,F1,G4,F5,E6,D7,C8"),
                                           theBishop.ValidMoves(m_emptyBoard), 
                                           "Expected bishop to be able to take the other bishop on C8");
	    }

        [Test]
        public void EnPassantInFenBlackTarget()
        {
            // White takes via EP
            m_normalBoard.MovePiece(m_normalBoard.GetContents(Location.B2), Location.B4);
            Console.WriteLine(m_normalBoard);
            m_normalBoard.MovePiece(m_normalBoard.GetContents(Location.E7), Location.E5);
            Console.WriteLine(m_normalBoard);
            m_normalBoard.MovePiece(m_normalBoard.GetContents(Location.B4), Location.B5);
            Console.WriteLine(m_normalBoard);
            m_normalBoard.MovePiece(m_normalBoard.GetContents(Location.A7), Location.A5);
            Console.WriteLine(m_normalBoard);
            Assert.AreEqual("rnbqkbnr/1ppp1ppp/8/pP2p3/8/8/P1PPPPPP/RNBQKBNR w KQkq A6", m_normalBoard.ToFen(), "Board does not look as expected with pending EP capture");
        }

		[Test]
		public void PromotionByType()
		{
			m_emptyBoard.AddPiece(new BlackKing(Location.A8));
			var theKing = new WhiteKing(Location.E1);
			m_emptyBoard.AddPiece(theKing);
			var pawn = new WhitePawn(Location.H7);
			m_emptyBoard.AddPiece(pawn);
			m_emptyBoard.MovePiece(pawn, Location.H8);
			m_emptyBoard.PromotePiece(PieceType.WhiteQueen);
			Console.WriteLine(m_emptyBoard);
			Assert.True(m_emptyBoard.KingInCheck(),"King should be in check after promotion of pawn");
		}

        [TestCase("Queen", Location.A8)]
        [TestCase("Rook", Location.A8)]
        [TestCase("Bishop", Location.F7)]
        [TestCase("Knight", Location.E7)]
		public void PromotionByString(string target, Location kingLocation)
		{
            m_emptyBoard.AddPiece(new BlackKing(kingLocation));
			var theKing = new WhiteKing(Location.E1);
			m_emptyBoard.AddPiece(new BlackRook(Location.G8)); // The rook gets taken as part of the promotion
			m_emptyBoard.AddPiece(theKing);
			var pawn = new WhitePawn(Location.H7);
			m_emptyBoard.AddPiece(pawn);
			m_emptyBoard.MovePiece(pawn, Location.G8);
            m_emptyBoard.PromotePiece(target);
			Console.WriteLine(m_emptyBoard);
			Assert.True(m_emptyBoard.KingInCheck(), "King should be in check after promotion of pawn");
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
            Assert.Throws(typeof(CannotPromoteException), () => PromotionByString(pieceType, Location.A8));
        }

		[TestCase("7r/2k5/8/8/8/5q2/8/6K1/ w - -")]
        [TestCase("7r/2k5/2P5/8/8/5q2/8/6K1/ w - -")]
		public void StaleMateTest(string fen)
		{
			m_emptyBoard.FromFen(fen);
			Console.WriteLine(m_emptyBoard);
			var king = m_emptyBoard.GetContents(Location.G1);
            Console.WriteLine(LocationListAsFriendlyString(king.ValidMoves(m_emptyBoard)));
			Assert.That(m_emptyBoard.IsStalemate(),"Should be stalemate - White has no moves");
		}

        [TestCase("7r/2k5/8/8/8/8/8/6K1/ w - -")]
        [TestCase("7r/2k5/2P5/8/8/8/8/6K1/ w - -")]
        public void NotStaleMateTest(string fen)
        {
            m_emptyBoard.FromFen(fen);
            Console.WriteLine(m_emptyBoard);
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

			m_normalBoard.MovePiece(m_normalBoard.GetContents(Location.F2), Location.F3);
			Console.WriteLine(m_normalBoard);
			m_normalBoard.MovePiece(m_normalBoard.GetContents(Location.E7), Location.E5);
			Console.WriteLine(m_normalBoard);
			m_normalBoard.MovePiece(m_normalBoard.GetContents(Location.G2), Location.G4);
			Console.WriteLine(m_normalBoard);
			m_normalBoard.MovePiece(m_normalBoard.GetContents(Location.D8), Location.H4);
			Console.WriteLine(m_normalBoard);
			Assert.True(m_normalBoard.IsCheckmate(),"White king should be mated");
		}

		[Test]
		public void MayCastleFailModes()
		{
			CastlingRules rules = new CastlingRules();
			// Cast to invalid enum values to exercise this code and prevent annoying complaints from NCover
			Assert.Throws(typeof(ArgumentException),() => rules.MayCastle((PieceColor) 125, Side.QueenSide, m_emptyBoard));
			Assert.Throws(typeof(ArgumentException), () => rules.MayCastle(PieceColor.White, (Side)44, m_emptyBoard));
			Assert.Throws(typeof(ArgumentException), () => rules.MayCastle(PieceColor.Black, (Side)44, m_emptyBoard));
		}

		[Test]
		public void IsMate()
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
            Assert.IsTrue(!isMate, "Given position should not be checkmate because the king is not in check");
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

			Console.WriteLine(m_normalBoard);

			Assert.AreEqual("1nbqkbn1/rppppppr/p6p/8/8/P6P/RPPPPPPR/1NBQKBN1 w - -", m_normalBoard.ToFen(), "Expected no castling to be allowed after moving all the rooks");
		}

        [Test]
        public void EarlyExitFromCastlingUpdateFunction()
        {
            m_emptyBoard.FromFen("1nbqkbn1/rppppppr/p6p/8/8/P6P/RPPPPPPR/1NBQKBN1 w - -");
            m_emptyBoard.Move(Location.B2, Location.B3);
            // Exercises the early exit from Update in the castling rules class
        }

		[Test]
		public void CancelCastlingByTakingRooks()
		{
			m_emptyBoard.FromFen("r3k2r/Rq5R/8/8/8/8/rQ5r/R3K2R/ w KQkq -");
			Console.WriteLine(m_emptyBoard);
			var blackKing = m_emptyBoard.GetContents(Location.E8) as BlackKing;
			var whiteKing = m_emptyBoard.GetContents(Location.E1) as WhiteKing;
			// Can castle everywhere
			Assert.True(m_emptyBoard.MayCastle(whiteKing, Side.KingSide));
			Assert.True(m_emptyBoard.MayCastle(whiteKing, Side.QueenSide));
			Assert.True(m_emptyBoard.MayCastle(blackKing, Side.KingSide));
			Assert.True(m_emptyBoard.MayCastle(blackKing, Side.QueenSide));

            Console.WriteLine("foo");

			m_emptyBoard.Move(Location.A7,Location.A8);
            Console.WriteLine(m_emptyBoard);
            bool ok = m_emptyBoard.Move(Location.B7, Location.B8);
            Assert.True(ok);
            Console.WriteLine(m_emptyBoard);
			m_emptyBoard.Move(Location.A8, Location.A7);
            Console.WriteLine(m_emptyBoard);
			m_emptyBoard.Move(Location.B8, Location.B7);
			Console.WriteLine(m_emptyBoard);
			Assert.True(m_emptyBoard.MayCastle(blackKing, Side.KingSide));
			Assert.False(m_emptyBoard.MayCastle(blackKing, Side.QueenSide));

			m_emptyBoard.CurrentTurn = PieceColor.Black;
			m_emptyBoard.Move(Location.A2, Location.A1);
			m_emptyBoard.Move(Location.B2, Location.B1);
			m_emptyBoard.Move(Location.A1, Location.A2);
			m_emptyBoard.Move(Location.B1, Location.B2);
			m_emptyBoard.Move(Location.B2, Location.B3);
			Console.WriteLine(m_emptyBoard);
			Assert.True(m_emptyBoard.MayCastle(whiteKing, Side.KingSide));
			Assert.False(m_emptyBoard.MayCastle(whiteKing, Side.QueenSide));
		}

		[Test]
		public void EscapableMate()
		{
			// Mate isn't just that the King can't do anything, it's that NO pieces can do anything. In this case, the white bishop can take the rook.
			m_emptyBoard.CurrentTurn = PieceColor.White;
			m_emptyBoard.AddPiece(new BlackKing(Location.H8));
			m_emptyBoard.AddPiece(new BlackRook(Location.H1));
			m_emptyBoard.AddPiece(new BlackRook(Location.H2));
			m_emptyBoard.AddPiece(new WhiteBishop(Location.A8));
			m_emptyBoard.AddPiece(new WhiteKing(Location.E1));

			Console.WriteLine(m_emptyBoard);

			Assert.That(m_emptyBoard.KingInCheck(),"King should be in check");
            Console.WriteLine(LocationListAsFriendlyString(m_emptyBoard.GetContents(Location.E1).ReachableSquares(m_emptyBoard)));
            Console.WriteLine(LocationListAsFriendlyString(m_emptyBoard.GetContents(Location.E1).ValidMoves(m_emptyBoard)));
            Assert.False(m_emptyBoard.GetContents(Location.E1).ValidMoves(m_emptyBoard).Any(), "King should have nowhere to go");
            Console.WriteLine(m_emptyBoard.GetContents(Location.A8));
            Console.WriteLine(LocationListAsFriendlyString(m_emptyBoard.GetContents(Location.A8).ValidMoves(m_emptyBoard)));
            Assert.True(m_emptyBoard.GetContents(Location.A8).ValidMoves(m_emptyBoard).Any(), "Bishop has many moves");
			Assert.False(m_emptyBoard.IsCheckmate(), "White king should not be mated");
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
		public void MoveAPieceInvalidly()
		{
			Assert.False(m_normalBoard.Move(Location.A2, Location.A1),"Should not be allowed to move a pawn onto own pieces");
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

