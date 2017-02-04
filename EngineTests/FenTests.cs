using NUnit.Framework;
using RedChess.ChessCommon.Enumerations;
using Redchess.Engine.Observers;

namespace Redchess.EngineTests
{
	[TestFixture]
	internal class FenTests : AbstractChessTest
	{
		[TestFixtureSetUp]
		public void SetupGame()
		{
			NormalBoard = new InteractiveBoard(PieceColor.White, false);
			EmptyBoard = new InteractiveBoard(PieceColor.White, true);
		}

		[Test]
		public void StartPosition()
		{
		    string fen = new Fen(NormalBoard).Value;
			FenAssert.AreEqual("rnbqkbnr/pppppppp/8/8/8/8/PPPPPPPP/RNBQKBNR w KQkq - 0", fen, "Fen representation of start position not as expected");
		}

        [Test]
        public void FenCaching()
        {
            string fen = NormalBoard.ToFen();
            string fromBoard = NormalBoard.ToFen();
            Assert.AreEqual(fromBoard, fen, "Expected two fen strings to be equal");
        }


		[Test]
		public void TwoKingsPosition()
		{
            EmptyBoard.FromFen("8/2K5/8/8/8/8/8/k7 w KQkq - 0");

		    string fen = new Fen(EmptyBoard).Value;
			// Castling settings are invalid here, ignore this
			FenAssert.AreEqual("8/2K5/8/8/8/8/8/k7 w KQkq - 0", fen, "Fen representation of position with two kings not as expected");
		}

		[Test]
		public void NoCastling()
		{
			NormalBoard.Move(Location.E2,Location.E3);
            NormalBoard.Move(Location.E7, Location.E6);
            NormalBoard.Move(Location.E1, Location.E2);
            NormalBoard.Move(Location.E8, Location.E7);

            string fen = NormalBoard.ToFen();
			// Castling settings are invalid here, ignore this
			FenAssert.AreEqual("rnbq1bnr/ppppkppp/4p3/8/8/4P3/PPPPKPPP/RNBQ1BNR w - - 2", fen, "Fen with no castling allowed not as expected");
		}

		[Test]
		public void WithEnPassantPosition()
		{
			// Move a pawn to make it an EP target
            NormalBoard.Move(Location.A2, Location.A4);
		    string fen = new Fen(NormalBoard).Value;
			FenAssert.AreEqual("rnbqkbnr/pppppppp/8/8/P7/8/1PPPPPPP/RNBQKBNR b KQkq A3 0", fen, "Fen representation of start position not as expected");
		}
	}
}

