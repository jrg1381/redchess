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
    }
}
