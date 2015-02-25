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
        public void PawnTakesPiece()
        {
            m_emptyBoard.FromFen("rnbqkbnr/ppp1pppp/8/3p4/4P3/8/PPPP1PPP/RNBQKBNR w KQkq - 0");
            var converter = new MoveTextConverter(m_emptyBoard);
            string move = converter.MoveAsText(m_emptyBoard.GetContents(Location.E4), Location.D5);
            Assert.AreEqual("exd5", move, "Pawn move was not correctly converted to text");
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
    }
}
