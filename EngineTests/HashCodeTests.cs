using NUnit.Framework;
using RedChess.ChessCommon.Enumerations;

namespace Redchess.EngineTests
{
    [TestFixture]
    internal class HashCodeTests : AbstractChessTest
    {
        [Test]
        public void StartPosition()
        {
            int hashCode1 = m_normalBoard.GetHashCode();
            int hashCode2 = m_normalBoard.GetHashCode();
            Assert.IsTrue(hashCode1 == hashCode2, "Expected hashcodes to be the same");
        }

        [Test]
        public void KingsPawnOpening()
        {
            int hashCode1 = m_normalBoard.GetHashCode();
            m_normalBoard.Move(Location.E2, Location.E4);
            int hashCode2 = m_normalBoard.GetHashCode();
            Assert.IsTrue(hashCode1 != hashCode2, "Expected hashcodes to differ");
        }

        [Test]
        public void MoveCounterIsIgnored()
        {
            var board1 = new InteractiveBoard(PieceColor.White, true);
            var board2 = new InteractiveBoard(PieceColor.White, true);

            board1.FromFen("rnbqkbnr/pppppppp/8/8/8/8/PPPPPPPP/RNBQKBNR w KQkq - 10");
            board2.FromFen("rnbqkbnr/pppppppp/8/8/8/8/PPPPPPPP/RNBQKBNR w KQkq - 0");
            var h1 = board1.GetHashCode();
            var h2 = board2.GetHashCode();

            Assert.IsTrue(h1 == h2, "Expected hashcodes to be the same");
        }

        [Test]
        public void CastlingNotIgnored()
        {
            var board1 = new InteractiveBoard(PieceColor.White, true);
            var board2 = new InteractiveBoard(PieceColor.White, true);

            board1.FromFen("rnbqkbnr/pppppppp/8/8/8/8/PPPPPPPP/RNBQKBNR w KQ - 0");
            board2.FromFen("rnbqkbnr/pppppppp/8/8/8/8/PPPPPPPP/RNBQKBNR w KQkq - 0");
            var h1 = board1.GetHashCode();
            var h2 = board2.GetHashCode();

            Assert.IsTrue(h1 != h2, "Expected hashcodes to differ");
        }

        [Test]
        public void EnPassantNotIgnored()
        {
            var board1 = new InteractiveBoard(PieceColor.White, true);
            var board2 = new InteractiveBoard(PieceColor.White, true);

            board1.FromFen("rnbqkbnr/1ppppppp/8/p/8/8/PPPPPPPP/RNBQKBNR w KQkq A3 0");
            board2.FromFen("rnbqkbnr/1ppppppp/8/p/8/8/PPPPPPPP/RNBQKBNR w KQkq - 0");
            var h1 = board1.GetHashCode();
            var h2 = board2.GetHashCode();

            Assert.IsTrue(h1 != h2, "Expected hashcodes to differ");
        }

        [Test]
        public void CurrentPlayerNotIgnored()
        {
            var board1 = new InteractiveBoard(PieceColor.White, true);
            var board2 = new InteractiveBoard(PieceColor.White, true);

            board1.FromFen("rnbqkbnr/pppppppp/8/8/8/8/PPPPPPPP/RNBQKBNR b KQkq - 0");
            board2.FromFen("rnbqkbnr/pppppppp/8/8/8/8/PPPPPPPP/RNBQKBNR w KQkq - 0");
            var h1 = board1.GetHashCode();
            var h2 = board2.GetHashCode();

            Assert.IsTrue(h1 != h2, "Expected hashcodes to differ");
        }

        [Test]
        public void BoardPositionNotIgnored()
        {
            var board1 = new InteractiveBoard(PieceColor.White, true);
            var board2 = new InteractiveBoard(PieceColor.White, true);

            board1.FromFen("k6K/8/8/8/8/8/8/8 w - - 0");
            board2.FromFen("K6k/8/8/8/8/8/8/8 w - - 0");
            var h1 = board1.GetHashCode();
            var h2 = board2.GetHashCode();

            Assert.AreNotEqual(h1, h2, "Expected hashcodes to differ");
        }
    }
}