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
            
            var moveDefinition = new BoardWithNextMove
            {
                MovedPiece = m_normalBoard.GetContents(Location.D2),
                Target = Location.E4,
                Board = m_normalBoard
            };

            string move = converter.MoveAsText(moveDefinition);
            Assert.AreEqual("e4",move, "Pawn move was not correctly converted to text");
        }

        [Test]
        public void PawnPromotes()
        {
            m_emptyBoard.FromFen("8/P7/8/8/8/8/8/1k5K w - - 0");
            var converter = new MoveTextConverter(m_emptyBoard);

            var moveDefinition = new BoardWithNextMove
            {
                MovedPiece = m_normalBoard.GetContents(Location.A7),
                Target = Location.A8,
                Promotion = "Q",
                Board = m_emptyBoard
            };

            string move = converter.MoveAsText(moveDefinition);
            Assert.AreEqual("a8(=Q)", move, "Pawn on a7 moves to a8 and promotes to queen"); 
        }

        [Test]
        public void AmbiguousPawnPromotes()
        {
            m_emptyBoard.FromFen("1q6/P1P5/8/8/8/8/8/k6K w - - 0");
            var converter = new MoveTextConverter(m_emptyBoard);

            var moveDefinition = new BoardWithNextMove
            {
                MovedPiece = m_normalBoard.GetContents(Location.A7),
                Target = Location.B8,
                Promotion = "Q",
                Board = m_emptyBoard
            };

            string move = converter.MoveAsText(moveDefinition);
            Assert.AreEqual("axb8(=Q)", move, "Pawn on a7 moves to a8 and promotes to queen");
        }

        [Test]
        public void AmbiguousPawnPromotesAndChecks()
        {
            m_emptyBoard.FromFen("1q5k/P1P5/8/8/8/8/8/7K w - - 0");
            var converter = new MoveTextConverter(m_emptyBoard);

            var moveDefinition = new BoardWithNextMove
            {
                MovedPiece = m_normalBoard.GetContents(Location.A7),
                Target = Location.B8,
                Promotion = "Q",
                Board = m_emptyBoard
            };
            
            string move = converter.MoveAsText(moveDefinition);
            Assert.AreEqual("axb8(=Q)+", move, "Pawn on a7 moves to a8 and promotes to queen");
        }

        [Test]
        public void NormalMoveOfKing()
        {
            m_emptyBoard.FromFen("rnbqkbnr/ppp1pppp/8/8/8/8/PPPP1PPP/RNB1KBNR w KQkq - 0");
            var converter = new MoveTextConverter(m_emptyBoard);

            var moveDefinition = new BoardWithNextMove
            {
                MovedPiece = m_emptyBoard.GetContents(Location.E1),
                Target = Location.D1,
                Board = m_emptyBoard
            };

            string move = converter.MoveAsText(moveDefinition);
            Assert.AreEqual("Kd1", move, "King moves to D1"); 
        }

        [Test]
        public void PawnTakesPiece()
        {
            m_emptyBoard.FromFen("rnbqkbnr/ppp1pppp/8/3p4/4P3/8/PPPP1PPP/RNBQKBNR w KQkq - 0");
            var converter = new MoveTextConverter(m_emptyBoard);
            
            var moveDefinition = new BoardWithNextMove
            {
                MovedPiece = m_emptyBoard.GetContents(Location.E4),
                Target = Location.D5,
                Board = m_emptyBoard
            };

            string move = converter.MoveAsText(moveDefinition);
            Assert.AreEqual("exd5", move, "Pawn move was not correctly converted to text");
        }

        [Test]
        public void PawnTakesEnPassant()
        {
            m_emptyBoard.FromFen("rnbqkbnr/pppp1ppp/8/8/P2Pp3/8/1PP1PPPP/RNBQKBNR b KQkq D3 0");
            var converter = new MoveTextConverter(m_emptyBoard);

            var moveDefinition = new BoardWithNextMove
            {
                MovedPiece = m_emptyBoard.GetContents(Location.E4),
                Target = Location.D3,
                Board = m_emptyBoard
            };

            string move = converter.MoveAsText(moveDefinition);
            Assert.AreEqual("exd3", move, "Pawn move was not correctly converted to text");
        }

        [Test]
        public void PieceMovesWithCheck()
        {
            m_emptyBoard.FromFen("rnbqkbnr/ppp1pppp/8/3b4/4Q3/8/PPPP1PPP/RNBQKBNR w KQkq - 0");
            var converter = new MoveTextConverter(m_emptyBoard);

            var moveDefinition = new BoardWithNextMove
            {
                MovedPiece = m_emptyBoard.GetContents(Location.E4),
                Target = Location.A4,
                Board = m_emptyBoard
            };

            string move = converter.MoveAsText(moveDefinition);
            Assert.AreEqual("Qa4+", move, "Queen moves to A4");
        }

        [Test]
        public void PieceMovesWithMate()
        {
            m_emptyBoard.FromFen("k7/7R/6R1/8/8/8/8/K7 w - - 0");
            var converter = new MoveTextConverter(m_emptyBoard);

            var moveDefinition = new BoardWithNextMove
            {
                MovedPiece = m_emptyBoard.GetContents(Location.G6),
                Target = Location.G8,
                Board = m_emptyBoard
            };

            string move = converter.MoveAsText(moveDefinition);
            Assert.AreEqual("Rg8#", move, "Rook moves to g8 and mate");
        }

        [Test]
        public void PieceMoves()
        {
            m_emptyBoard.FromFen("rnbqkbnr/ppp1pppp/8/3b4/4Q3/8/PPPP1PPP/RNBQKBNR w KQkq - 0");
            var converter = new MoveTextConverter(m_emptyBoard);

            var moveDefinition = new BoardWithNextMove
            {
                MovedPiece = m_emptyBoard.GetContents(Location.E4),
                Target = Location.B4,
                Board = m_emptyBoard
            };

            string move = converter.MoveAsText(moveDefinition);
            Assert.AreEqual("Qb4", move, "Queen moves to B4");
        }

        [Test]
        public void PieceTakesPiece()
        {
            m_emptyBoard.FromFen("rnbqkbnr/ppp1pppp/8/3b4/4Q3/8/PPPP1PPP/RNBQKBNR w KQkq - 0");
            var converter = new MoveTextConverter(m_emptyBoard);

            var moveDefinition = new BoardWithNextMove
            {
                MovedPiece = m_emptyBoard.GetContents(Location.E4),
                Target = Location.D5,
                Board = m_emptyBoard
            };

            string move = converter.MoveAsText(moveDefinition);
            Assert.AreEqual("Qxd5", move, "Queen takes bishop on D5");
        }

        [Test]
        public void CastleKingSide()
        {
            m_emptyBoard.FromFen("k7/8/8/8/8/8/8/4K2R w KQkq -");
            var converter = new MoveTextConverter(m_emptyBoard);

            var moveDefinition = new BoardWithNextMove
            {
                MovedPiece = m_emptyBoard.GetContents(Location.E1),
                Target = Location.G1,
                Board = m_emptyBoard
            };

            string move = converter.MoveAsText(moveDefinition);
            Assert.AreEqual("O-O", move, "White king castles kingside");
        }

        [Test]
        public void CastleQueenSide()
        {
            m_emptyBoard.FromFen("3k4/8/8/8/8/8/8/R3K2R w KQ -");
            var converter = new MoveTextConverter(m_emptyBoard);

            var moveDefinition = new BoardWithNextMove
            {
                MovedPiece = m_emptyBoard.GetContents(Location.E1),
                Target = Location.C1,
                Board = m_emptyBoard
            };

            string move = converter.MoveAsText(moveDefinition);
            Assert.AreEqual("O-O-O+", move, "White king castles queenside");
        }

        [Test]
        public void AmbiguousMoveByRow()
        {
            m_emptyBoard.FromFen("1k4K1/8/8/8/8/8/8/R6R w - -");
            var converter = new MoveTextConverter(m_emptyBoard);

            var moveDefinition = new BoardWithNextMove
            {
                MovedPiece = m_emptyBoard.GetContents(Location.A1),
                Target = Location.C1,
                Board = m_emptyBoard
            };

            string move = converter.MoveAsText(moveDefinition);
            Assert.AreEqual("Rac1", move, "Rook on A1 moves to C1"); 
        }

        [Test]
        public void AmbiguousMoveByColumn()
        {
            m_emptyBoard.FromFen("R7/8/8/7k/8/7K/8/R7 w - -");
            var converter = new MoveTextConverter(m_emptyBoard);

            var moveDefinition = new BoardWithNextMove
            {
                MovedPiece = m_emptyBoard.GetContents(Location.A1),
                Target = Location.A3,
                Board = m_emptyBoard
            };

            string move = converter.MoveAsText(moveDefinition);
            Assert.AreEqual("R1a3", move, "Rook on A1 moves to a3");
        }
    }
}
