using NUnit.Framework;
using RedChess.ChessCommon;
using RedChess.ChessCommon.Enumerations;

namespace Redchess.EngineTests
{
    [TestFixture]
    internal class EnPassantTests : AbstractChessTest
    {
        [Test]
        public void EnPassantTargetsAreCreatedAfterInitialPawnMoves()
        {
            NormalBoard.Move(Location.D2, Location.D4);
            Assert.AreEqual(Location.D3, NormalBoard.EnPassantTarget, "D3 should be an e.p. target");
            
            NormalBoard.Move(Location.D7, Location.D5);
            Assert.AreEqual(Location.D6, NormalBoard.EnPassantTarget, "D3 should be an e.p. target");
            
        }

        [Test]
        public void BlackCanCaptureEnPassant()
        {
            // Black takes via EP
            NormalBoard.Move(Location.A2, Location.A3);            
            NormalBoard.Move(Location.E7, Location.E5);
            NormalBoard.Move(Location.A3, Location.A4);
            NormalBoard.Move(Location.E5, Location.E4);
            NormalBoard.Move(Location.D2, Location.D4);
            NormalBoard.Move(Location.E4, Location.D3);
            
            FenAssert.AreEqual("rnbqkbnr/pppp1ppp/8/8/P7/3p4/1PP1PPPP/RNBQKBNR w KQkq - 0", NormalBoard.ToFen(), "Board does not look as expected after black EP capture");
        }

        [Test]
        public void WhiteCanCaptureEnPassant()
        {
            // White takes via EP
            NormalBoard.Move(Location.B2, Location.B4);
            NormalBoard.Move(Location.E7, Location.E5);
            NormalBoard.Move(Location.B4, Location.B5);
            NormalBoard.Move(Location.A7, Location.A5);
            NormalBoard.Move(Location.B5, Location.A6);
            
            FenAssert.AreEqual("rnbqkbnr/1ppp1ppp/P7/4p3/8/8/P1PPPPPP/RNBQKBNR b KQkq - 0", NormalBoard.ToFen(), "Board does not look as expected after white EP capture");
        }

        [Test]
        public void EnPassantInFenWhiteTarget()
        {
            // Black takes via EP
            NormalBoard.Move(Location.A2, Location.A3);
            NormalBoard.Move(Location.E7, Location.E5);
            NormalBoard.Move(Location.A3, Location.A4);
            NormalBoard.Move(Location.E5, Location.E4);
            NormalBoard.Move(Location.D2, Location.D4);
            
            FenAssert.AreEqual("rnbqkbnr/pppp1ppp/8/8/P2Pp3/8/1PP1PPPP/RNBQKBNR b KQkq D3 0", NormalBoard.ToFen(), "Board does not look as expected with pending EP capture");
        }

        [Test]
        public void EnPassantDoesNotBlockTaking()
        {
            // This was a problem which happened when the e.p. bitmaps were included in the calculation of the occupied squares, causing the invisible ep target to block other pieces.
            EmptyBoard.FromFen("rnbqkbnr/ppp2ppp/3p4/4P3/8/6PB/PPPPPP1P/RNBQK1NR w KQkq E6 1");
            var theBishop = EmptyBoard.GetContents(Location.H3);
            Assert.That(theBishop.Type.IsOfType(PieceType.WhiteBishop), "The white bishop is not a white bishop! " + theBishop.Type);
            var expectedValidMoves = new[]
            {
                Location.G2, Location.F1, Location.G4, Location.F5, Location.E6, Location.D7, Location.C8
            };
            CollectionAssert.AreEquivalent(expectedValidMoves, theBishop.ValidMoves(EmptyBoard), "Expected bishop to be able to take the other bishop on C8");
        }

        [Test]
        public void EnPassantInFenBlackTarget()
        {
            // White takes via EP
            NormalBoard.Move(Location.B2, Location.B4);
            NormalBoard.Move(Location.E7, Location.E5);
            NormalBoard.Move(Location.B4, Location.B5);
            NormalBoard.Move(Location.A7, Location.A5);
            
            FenAssert.AreEqual("rnbqkbnr/1ppp1ppp/8/pP2p3/8/8/P1PPPPPP/RNBQKBNR w KQkq A6 0", NormalBoard.ToFen(), "Board does not look as expected with pending EP capture");
        }
    }
}