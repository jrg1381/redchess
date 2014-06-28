using System;
using NUnit.Framework;
using RedChess.ChessCommon.Enumerations;
using Redchess.Engine;
using Redchess.Engine.Pieces.Black;
using Redchess.Engine.Pieces.White;

namespace EngineTests
{
    [TestFixture]
    internal class CastlingTests : AbstractChessTest
    {
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
            m_emptyBoard.Move(Location.E1, Location.E2);
            // Let white move twice
            m_emptyBoard.CurrentTurn = PieceColor.White;
            m_emptyBoard.Move(Location.E1, Location.E1);
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
            m_emptyBoard.Move(Location.A8, theRook1.Position.Location);
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
            m_emptyBoard.Move(Location.E8, Location.G8);
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
            m_emptyBoard.Move(Location.E1, Location.C1);
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
            m_emptyBoard.Move(Location.E8, Location.C8);
            Assert.False(m_emptyBoard.MayCastle(theKing, Side.QueenSide), "Castling has occurred already, not allowed again");
            Console.WriteLine(m_emptyBoard);
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

            m_emptyBoard.Move(Location.A7, Location.A8);
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
    }
}