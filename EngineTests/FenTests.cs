using NUnit.Framework;
using RedChess.ChessCommon.Enumerations;
using Redchess.Engine;

namespace Redchess.EngineTests
{
	[TestFixture]
	internal class FenTests : AbstractChessTest
	{
		[TestFixtureSetUp]
		public void SetupGame()
		{
			m_normalBoard = new InteractiveBoard(PieceColor.White, false);
			m_emptyBoard = new InteractiveBoard(PieceColor.White, true);
		}

		[Test]
		public void StartPosition()
		{
			string fen = new Fen(m_normalBoard, new CastlingRules()).ToFen();
			FenAssert.AreEqual("rnbqkbnr/pppppppp/8/8/8/8/PPPPPPPP/RNBQKBNR w KQkq -", fen, "Fen representation of start position not as expected");
		}

		[Test]
		public void TwoKingsPosition()
		{
            m_emptyBoard.FromFen("8/2K5/8/8/8/8/8/k7 w KQkq -");

            string fen = new Fen(m_emptyBoard, new CastlingRules()).ToFen();
			// Castling settings are invalid here, ignore this
			FenAssert.AreEqual("8/2K5/8/8/8/8/8/k7 w KQkq -", fen, "Fen representation of position with two kings not as expected");
		}

		[Test]
		public void NoCastling()
		{
			m_normalBoard.Move(Location.E2,Location.E3);
            m_normalBoard.Move(Location.E7, Location.E6);
            m_normalBoard.Move(Location.E1, Location.E2);
            m_normalBoard.Move(Location.E8, Location.E7);

            string fen = m_normalBoard.ToFen();
			// Castling settings are invalid here, ignore this
			FenAssert.AreEqual("rnbq1bnr/ppppkppp/4p3/8/8/4P3/PPPPKPPP/RNBQ1BNR w - -", fen, "Fen with no castling allowed not as expected");
		}

		[Test]
		public void WithEnPassantPosition()
		{
			// Move a pawn to make it an EP target
            m_normalBoard.Move(Location.A2, Location.A4);
            string fen = new Fen(m_normalBoard, new CastlingRules()).ToFen();
			FenAssert.AreEqual("rnbqkbnr/pppppppp/8/8/P7/8/1PPPPPPP/RNBQKBNR b KQkq A3", fen, "Fen representation of start position not as expected");
		}
	}
}

