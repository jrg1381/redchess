using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NUnit.Framework;
using Redchess.Engine;
using RedChess.ChessCommon.Enumerations;

namespace Redchess.EngineTests
{
    [TestFixture]
    class MoveConverterTests : AbstractChessTest
    {
        [Test]
        public void FirstMoveOfPawn()
        {
            var converter = new MoveTextConverter(m_normalBoard);
            string move = converter.MoveAsText(m_normalBoard.GetContents(Location.E2), Location.E4);
            Assert.AreEqual("e4",move, "Pawn move was not correctly converted to text");
        }

        [Test]
        public void PawnPromotes()
        {
            m_emptyBoard.FromFen("8/P7/8/8/8/8/8/k6K w - - 0");
            var converter = new MoveTextConverter(m_emptyBoard);
            string move = converter.MoveAsText(m_emptyBoard.GetContents(Location.A7), Location.A8, "Q");
            Assert.AreEqual("a8(=Q)", move, "Pawn on a7 moves to a8 and promotes to queen"); 
        }

        [Test]
        public void AmbiguousPawnPromotes()
        {
            m_emptyBoard.FromFen("1q6/P1P5/8/8/8/8/8/k6K w - - 0");
            var converter = new MoveTextConverter(m_emptyBoard);
            string move = converter.MoveAsText(m_emptyBoard.GetContents(Location.A7), Location.B8, "Q");
            Assert.AreEqual("axb8(=Q)", move, "Pawn on a7 moves to a8 and promotes to queen");
        }

        [Test]
        public void AmbiguousPawnPromotesAndChecks()
        {
            m_emptyBoard.FromFen("1q5k/P1P5/8/8/8/8/8/7K w - - 0");
            var converter = new MoveTextConverter(m_emptyBoard);
            string move = converter.MoveAsText(m_emptyBoard.GetContents(Location.A7), Location.B8, "Q");
            Assert.AreEqual("axb8(=Q)+", move, "Pawn on a7 moves to a8 and promotes to queen");
        }

        [Test]
        public void NormalMoveOfKing()
        {
            m_emptyBoard.FromFen("rnbqkbnr/ppp1pppp/8/8/8/8/PPPP1PPP/RNB1KBNR w KQkq - 0");
            var converter = new MoveTextConverter(m_emptyBoard);
            string move = converter.MoveAsText(m_emptyBoard.GetContents(Location.E1), Location.D1);
            Assert.AreEqual("Kd1", move, "King moves to D1"); 
        }

        [Test]
        public void PawnTakesPiece()
        {
            m_emptyBoard.FromFen("rnbqkbnr/ppp1pppp/8/3p4/4P3/8/PPPP1PPP/RNBQKBNR w KQkq - 0");
            var converter = new MoveTextConverter(m_emptyBoard);
            string move = converter.MoveAsText(m_emptyBoard.GetContents(Location.E4), Location.D5);
            Assert.AreEqual("exd5", move, "Pawn move was not correctly converted to text");
        }

        [Test]
        public void PawnTakesEnPassant()
        {
            m_emptyBoard.FromFen("rnbqkbnr/pppp1ppp/8/8/P2Pp3/8/1PP1PPPP/RNBQKBNR b KQkq D3 0");
            var converter = new MoveTextConverter(m_emptyBoard);
            string move = converter.MoveAsText(m_emptyBoard.GetContents(Location.E4), Location.D3);
            Assert.AreEqual("exd3", move, "Pawn move was not correctly converted to text");
        }

        [Test]
        public void PieceMovesWithCheck()
        {
            m_emptyBoard.FromFen("rnbqkbnr/ppp1pppp/8/3b4/4Q3/8/PPPP1PPP/RNBQKBNR w KQkq - 0");
            var converter = new MoveTextConverter(m_emptyBoard);
            string move = converter.MoveAsText(m_emptyBoard.GetContents(Location.E4), Location.A4);
            Assert.AreEqual("Qa4+", move, "Queen moves to A4");
        }

        [Test]
        public void PieceMovesWithMate()
        {
            m_emptyBoard.FromFen("k7/7R/6R1/8/8/8/8/K7 w - - 0");
            var converter = new MoveTextConverter(m_emptyBoard);
            string move = converter.MoveAsText(m_emptyBoard.GetContents(Location.G6), Location.G8);
            Assert.AreEqual("Rg8#", move, "Rook moves to g8 and mate");
        }

        [Test]
        public void PieceMoves()
        {
            m_emptyBoard.FromFen("rnbqkbnr/ppp1pppp/8/3b4/4Q3/8/PPPP1PPP/RNBQKBNR w KQkq - 0");
            var converter = new MoveTextConverter(m_emptyBoard);
            string move = converter.MoveAsText(m_emptyBoard.GetContents(Location.E4), Location.B4);
            Assert.AreEqual("Qb4", move, "Queen moves to B4");
        }

        [Test]
        public void PieceTakesPiece()
        {
            m_emptyBoard.FromFen("rnbqkbnr/ppp1pppp/8/3b4/4Q3/8/PPPP1PPP/RNBQKBNR w KQkq - 0");
            var converter = new MoveTextConverter(m_emptyBoard);
            string move = converter.MoveAsText(m_emptyBoard.GetContents(Location.E4), Location.D5);
            Assert.AreEqual("Qxd5", move, "Queen takes bishop on D5");
        }

        [Test]
        public void CastleKingSide()
        {
            m_emptyBoard.FromFen("8/8/8/8/8/8/8/4K2R w KQkq -");
            var converter = new MoveTextConverter(m_emptyBoard);
            string move = converter.MoveAsText(m_emptyBoard.GetContents(Location.E1), Location.G1);
            Assert.AreEqual("O-O", move, "White king castles kingside");
        }

        [Test]
        public void CastleQueenSide()
        {
            m_emptyBoard.FromFen("8/8/8/8/8/8/8/R3K2R w KQkq -");
            var converter = new MoveTextConverter(m_emptyBoard);
            string move = converter.MoveAsText(m_emptyBoard.GetContents(Location.E1), Location.C1);
            Assert.AreEqual("O-O-O", move, "White king castles queenside");
        }

        [Test]
        public void AmbiguousMoveByRow()
        {
            m_emptyBoard.FromFen("1k4K1/8/8/8/8/8/8/R6R w - -");
            var converter = new MoveTextConverter(m_emptyBoard);
            string move = converter.MoveAsText(m_emptyBoard.GetContents(Location.A1), Location.C1);
            Assert.AreEqual("Rac1", move, "Rook on A1 moves to C1"); 
        }

        [Test]
        public void AmbiguousMoveByColumn()
        {
            m_emptyBoard.FromFen("R7/8/8/7k/8/7K/8/R7 w - -");
            var converter = new MoveTextConverter(m_emptyBoard);
            string move = converter.MoveAsText(m_emptyBoard.GetContents(Location.A1), Location.A3);
            Assert.AreEqual("R1a3", move, "Rook on A1 moves to a3");
        }

        [Test]
        public void AmbiguousMoveBoth()
        {
            m_emptyBoard.FromFen("1R6/R5R1/8/7k/8/7K/8/1R6 w - -");
            var converter = new MoveTextConverter(m_emptyBoard);
            string move = converter.MoveAsText(m_emptyBoard.GetContents(Location.B1), Location.B7);
            Assert.AreEqual("Rb1b7", move, "Rook on B1 moves to B7");
        }
    }
}
