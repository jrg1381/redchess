using NUnit.Framework;
using RedChess.ChessCommon.Enumerations;
using Redchess.Engine.Interfaces;

namespace Redchess.EngineTests
{
    [TestFixture]
    internal class CastlingTests : AbstractChessTest
    {
        [Test]
        public void WhiteCastlesKingside()
        {
            EmptyBoard.FromFen("k7/8/8/8/8/8/8/4K2R w KQkq - 0");
            Assert.True(EmptyBoard.MayCastle(WhiteKingLocation(), Side.KingSide), "Untouched king and rook should be allowed to castle");
        }

        [Test]
        public void NoCastlingWhenInCheck()
        {
            EmptyBoard.FromFen("k7/8/8/8/4q3/8/8/4K2R w KQkq - 0");
            Assert.False(EmptyBoard.MayCastle(WhiteKingLocation(), Side.KingSide), "The king is in check right now! No castling allowed.");
        }

        [Test]
        public void NoCastlingPassingThroughCheck()
        {
            EmptyBoard.FromFen("k7/8/8/8/5q2/8/8/4K2R w KQkq - 0");
            Assert.False(EmptyBoard.MayCastle(WhiteKingLocation(), Side.KingSide), "The king would pass through check! No castling allowed.");
        }

        [Test]
        public void NoCastlingFinishingInCheck()
        {
            EmptyBoard.FromFen("k7/8/8/8/6q1/8/8/4K2R w KQkq - 0");
            Assert.False(EmptyBoard.MayCastle(WhiteKingLocation(), Side.KingSide), "The king would finish in check! No castling allowed.");
        }

        [Test]
        public void NoCastlingAfterKingHasMoved()
        {
            EmptyBoard.FromFen("k7/8/8/8/8/8/8/4K2R w KQkq - 0");
            EmptyBoard.Move(Location.E1, Location.E2);
            // Doesn't matter what black does
            EmptyBoard.Move(Location.A8, Location.B8);
            // Put the king back again
            EmptyBoard.Move(Location.E2, Location.E1);
            Assert.False(EmptyBoard.MayCastle(WhiteKingLocation(), Side.KingSide), "The king has been moved. No castling allowed.");
        }

        [Test]
        public void NoCastlingThroughAnotherPiece()
        {
            EmptyBoard.FromFen("k7/8/8/8/8/8/8/4Kb1R w KQkq - 0");
            Assert.False(EmptyBoard.MayCastle(WhiteKingLocation(), Side.KingSide), "The king is blocked by another piece. No castling.");
        }

        [Test]
        public void NoCastlingThroughAFriendlyPiece()
        {
            EmptyBoard.FromFen("k7/p7/8/8/8/8/8/RQ2K1QR w KQkq - 0");
            Assert.False(EmptyBoard.MayCastle(WhiteKingLocation(), Side.KingSide), "The king is blocked by another piece. No castling.");
            Assert.False(EmptyBoard.MayCastle(WhiteKingLocation(), Side.QueenSide), "The king is blocked by another piece. No castling.");
        }

        [Test]
        public void NoCastlingIfARookHasBeenTaken()
        {
            EmptyBoard.FromFen("b7/8/8/1k6/8/8/8/R3K2R b KQkq - 0");
            EmptyBoard.Move(Location.A8, Location.H1);
            Assert.False(EmptyBoard.MayCastle(WhiteKingLocation(), Side.KingSide), "The rook was taken by the bishop");
            Assert.True(EmptyBoard.MayCastle(WhiteKingLocation(), Side.QueenSide), "Should still be ok to castle queen side");
        }

        private IPiece WhiteKingLocation()
        {
            return EmptyBoard.GetContents(Location.E1);
        }

        [Test]
        public void FenAfterCastling()
        {
            EmptyBoard.FromFen("7k/8/8/8/8/8/8/4K2R w KQ - 0");
            Assert.True(EmptyBoard.MayCastle(WhiteKingLocation(), Side.KingSide), "Untouched white king and rook should be allowed to castle");
            EmptyBoard.Move(Location.E1,Location.G1);
            FenAssert.AreEqual("7k/8/8/8/8/8/8/5RK1 b - - 1", EmptyBoard.ToFen(), "Unexpected FEN after castling");
        }

        [Test]
        public void BlackCastlesKingside()
        {
            EmptyBoard.FromFen("4k2r/8/8/8/8/8/8/7K b KQkq - 0");
            Assert.True(EmptyBoard.MayCastle(BlackKingLocation(), Side.KingSide), "Untouched black king and rook should be allowed to castle");
            EmptyBoard.Move(Location.E8, Location.G8);
        }

        private IPiece BlackKingLocation()
        {
            return EmptyBoard.GetContents(Location.E8);
        }

        [Test]
        public void DoubleCastlingNotAllowedWhiteQueenSide()
        {
            EmptyBoard.FromFen("7k/8/8/8/8/8/8/R3K3 w KQkq - 0");
            Assert.True(EmptyBoard.MayCastle(WhiteKingLocation(), Side.QueenSide), "Untouched king and rook should be allowed to castle");
            EmptyBoard.Move(Location.E1, Location.C1);
            Assert.False(EmptyBoard.MayCastle(EmptyBoard.GetContents(Location.C1), Side.QueenSide), "Castling has occurred already, not allowed again");
        }

        [Test]
        public void DoubleCastlingNotAllowedBlackQueenSide()
        {
            EmptyBoard.FromFen("r3k3/8/8/8/8/8/8/K7 b KQkq - 0");
            Assert.True(EmptyBoard.MayCastle(BlackKingLocation(), Side.QueenSide), "Untouched king and rook should be allowed to castle");
            EmptyBoard.Move(Location.E8, Location.C8);
            Assert.False(EmptyBoard.MayCastle(EmptyBoard.GetContents(Location.C8), Side.QueenSide), "Castling has occurred already, not allowed again");
        }

        [Test]
        public void CancelCastlingByTakingRooks()
        {
            EmptyBoard.FromFen("r3k2r/Rq5R/8/8/8/8/rQ5r/R3K2R/ w KQkq - 0");

            var blackKing = BlackKingLocation();
            var whiteKing = WhiteKingLocation();
            // Can castle everywhere
            Assert.True(EmptyBoard.MayCastle(whiteKing, Side.KingSide));
            Assert.True(EmptyBoard.MayCastle(whiteKing, Side.QueenSide));
            Assert.True(EmptyBoard.MayCastle(blackKing, Side.KingSide));
            Assert.True(EmptyBoard.MayCastle(blackKing, Side.QueenSide));

            EmptyBoard.Move(Location.A7, Location.A8);
            EmptyBoard.Move(Location.B7, Location.B8);
            EmptyBoard.Move(Location.A8, Location.A7);
            EmptyBoard.Move(Location.B8, Location.B7);
            Assert.True(EmptyBoard.MayCastle(blackKing, Side.KingSide));
            Assert.False(EmptyBoard.MayCastle(blackKing, Side.QueenSide));

            EmptyBoard.Move(Location.B2, Location.B3);
            EmptyBoard.Move(Location.A2, Location.A1);
            EmptyBoard.Move(Location.B3, Location.B1);
            EmptyBoard.Move(Location.A1, Location.A2);
            EmptyBoard.Move(Location.B1, Location.B2);
            EmptyBoard.Move(Location.B7, Location.B2);

            Assert.True(EmptyBoard.MayCastle(whiteKing, Side.KingSide));
            Assert.False(EmptyBoard.MayCastle(whiteKing, Side.QueenSide));
        }
    }
}