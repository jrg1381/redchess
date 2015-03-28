using System;
using NUnit.Framework;
using RedChess.ChessCommon.Enumerations;
using Redchess.Engine;
using Redchess.Engine.Interfaces;

namespace Redchess.EngineTests
{
    [TestFixture]
    internal class CastlingTests : AbstractChessTest
    {
        [Test]
        public void CastlingTest1()
        {
            m_emptyBoard.FromFen("k7/8/8/8/8/8/8/4K2R w KQkq - 0");
            Assert.True(m_emptyBoard.MayCastle(WhiteKingLocation(), Side.KingSide), "Untouched king and rook should be allowed to castle");
        }

        [Test]
        public void CastlingTest2()
        {
            m_emptyBoard.FromFen("k7/8/8/8/4q3/8/8/4K2R w KQkq - 0");
            Assert.False(m_emptyBoard.MayCastle(WhiteKingLocation(), Side.KingSide), "The king is in check right now! No castling allowed.");
        }

        [Test]
        public void CastlingTest3()
        {
            m_emptyBoard.FromFen("k7/8/8/8/5q2/8/8/4K2R w KQkq - 0");
            Assert.False(m_emptyBoard.MayCastle(WhiteKingLocation(), Side.KingSide), "The king would pass through check! No castling allowed.");
        }

        [Test]
        public void CastlingTest4()
        {
            m_emptyBoard.FromFen("k7/8/8/8/6q1/8/8/4K2R w KQkq - 0");
            Assert.False(m_emptyBoard.MayCastle(WhiteKingLocation(), Side.KingSide), "The king would finish in check! No castling allowed.");
        }

        [Test]
        public void CastlingTest5()
        {
            m_emptyBoard.FromFen("k7/8/8/8/8/8/8/4K2R w KQkq - 0");
            m_emptyBoard.Move(Location.E1, Location.E2);
            // Doesn't matter what black does
            m_emptyBoard.Move(Location.A8, Location.B8);
            // Put the king back again
            m_emptyBoard.Move(Location.E2, Location.E1);
            Assert.False(m_emptyBoard.MayCastle(WhiteKingLocation(), Side.KingSide), "The king has been moved. No castling allowed.");
        }

        [Test]
        public void CastlingTest6()
        {
            m_emptyBoard.FromFen("k7/8/8/8/8/8/8/4Kb1R w KQkq - 0");
            Assert.False(m_emptyBoard.MayCastle(WhiteKingLocation(), Side.KingSide), "The king is blocked by another piece. No castling.");
        }

        [Test]
        public void CastlingTest7()
        {
            m_emptyBoard.FromFen("b7/8/8/1k6/8/8/8/R3K2R b KQkq - 0");
            m_emptyBoard.Move(Location.A8, Location.H1);
            Assert.False(m_emptyBoard.MayCastle(WhiteKingLocation(), Side.KingSide), "The rook was taken by the bishop");
            Assert.True(m_emptyBoard.MayCastle(WhiteKingLocation(), Side.QueenSide), "Should still be ok to castle queen side");
        }

        private IPiece WhiteKingLocation()
        {
            return m_emptyBoard.GetContents(Location.E1);
        }

        [Test]
        public void CastlingTest8()
        {
            m_emptyBoard.FromFen("7k/8/8/8/8/8/8/4K2R w KQ - 0");
            Assert.True(m_emptyBoard.MayCastle(WhiteKingLocation(), Side.KingSide), "Untouched white king and rook should be allowed to castle");
            m_emptyBoard.Move(Location.E1,Location.G1);
            FenAssert.AreEqual("7k/8/8/8/8/8/8/5RK1 b - - 1", m_emptyBoard.ToFen(), "Unexpected FEN after castling");
        }

        [Test]
        public void CastlingTest9()
        {
            m_emptyBoard.FromFen("4k2r/8/8/8/8/8/8/7K b KQkq - 0");
            Assert.True(m_emptyBoard.MayCastle(BlackKingLocation(), Side.KingSide), "Untouched black king and rook should be allowed to castle");
            m_emptyBoard.Move(Location.E8, Location.G8);
        }

        private IPiece BlackKingLocation()
        {
            return m_emptyBoard.GetContents(Location.E8);
        }

        [Test]
        public void CastlingTest10()
        {
            m_emptyBoard.FromFen("7k/8/8/8/8/8/8/R3K3 w KQkq - 0");
            Assert.True(m_emptyBoard.MayCastle(WhiteKingLocation(), Side.QueenSide), "Untouched king and rook should be allowed to castle");
            m_emptyBoard.Move(Location.E1, Location.C1);
            Assert.False(m_emptyBoard.MayCastle(m_emptyBoard.GetContents(Location.C1), Side.QueenSide), "Castling has occurred already, not allowed again");
        }

        [Test]
        public void CastlingTest11()
        {
            m_emptyBoard.FromFen("r3k3/8/8/8/8/8/8/K7 b KQkq - 0");
            Assert.True(m_emptyBoard.MayCastle(BlackKingLocation(), Side.QueenSide), "Untouched king and rook should be allowed to castle");
            m_emptyBoard.Move(Location.E8, Location.C8);
            Assert.False(m_emptyBoard.MayCastle(m_emptyBoard.GetContents(Location.C8), Side.QueenSide), "Castling has occurred already, not allowed again");
        }

        [Test]
        public void CancelCastlingByTakingRooks()
        {
            m_emptyBoard.FromFen("r3k2r/Rq5R/8/8/8/8/rQ5r/R3K2R/ w KQkq - 0");

            var blackKing = BlackKingLocation();
            var whiteKing = WhiteKingLocation();
            // Can castle everywhere
            Assert.True(m_emptyBoard.MayCastle(whiteKing, Side.KingSide));
            Assert.True(m_emptyBoard.MayCastle(whiteKing, Side.QueenSide));
            Assert.True(m_emptyBoard.MayCastle(blackKing, Side.KingSide));
            Assert.True(m_emptyBoard.MayCastle(blackKing, Side.QueenSide));

            m_emptyBoard.Move(Location.A7, Location.A8);
            m_emptyBoard.Move(Location.B7, Location.B8);
            m_emptyBoard.Move(Location.A8, Location.A7);
            m_emptyBoard.Move(Location.B8, Location.B7);
            Assert.True(m_emptyBoard.MayCastle(blackKing, Side.KingSide));
            Assert.False(m_emptyBoard.MayCastle(blackKing, Side.QueenSide));

            m_emptyBoard.Move(Location.B2, Location.B3);
            m_emptyBoard.Move(Location.A2, Location.A1);
            m_emptyBoard.Move(Location.B3, Location.B1);
            m_emptyBoard.Move(Location.A1, Location.A2);
            m_emptyBoard.Move(Location.B1, Location.B2);
            m_emptyBoard.Move(Location.B7, Location.B2);

            Assert.True(m_emptyBoard.MayCastle(whiteKing, Side.KingSide));
            Assert.False(m_emptyBoard.MayCastle(whiteKing, Side.QueenSide));
        }
    }
}