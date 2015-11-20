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
            var moveDefinition = new BoardStateTransition
            {
                MovedPiece = m_normalBoard.GetContents(Location.E2),
                Target = Location.E4,
                BoardBefore = new InteractiveBoard(m_normalBoard)
            };

            m_normalBoard.Move(moveDefinition.MovedPiece.Position.Location, moveDefinition.Target);
            moveDefinition.BoardAfter = new InteractiveBoard(m_normalBoard);
            var converter = new MoveTextConverter(moveDefinition);

            string move = converter.MoveAsText();
            Assert.AreEqual("e4",move, "Pawn move was not correctly converted to text");
        }

        [Test]
        public void FourKnights()
        {
            m_emptyBoard.FromFen("N1N5/8/8/8/N1N5/8/8/5k1K w - - 0");

            var moveDefinition = new BoardStateTransition
            {
                MovedPiece = m_emptyBoard.GetContents(Location.A8),
                Target = Location.B6,
                BoardBefore = new InteractiveBoard(m_emptyBoard)
            };

            m_emptyBoard.Move(moveDefinition.MovedPiece.Position.Location, moveDefinition.Target);
            moveDefinition.BoardAfter = new InteractiveBoard(m_emptyBoard);
            var converter = new MoveTextConverter(moveDefinition);
            string move = converter.MoveAsText();
            Assert.AreEqual("Na8b6", move, "Knight moves"); 
        }

        [Test]
        public void PawnPromotes()
        {
            m_emptyBoard.FromFen("8/P7/8/8/8/8/8/1k5K w - - 0");

            var moveDefinition = new BoardStateTransition
            {
                MovedPiece = m_emptyBoard.GetContents(Location.A7),
                Target = Location.A8,
                Promotion = "Q",
                BoardBefore = new InteractiveBoard(m_emptyBoard)
            };

            m_emptyBoard.Move(moveDefinition.MovedPiece.Position.Location, moveDefinition.Target);
            m_emptyBoard.PromotePiece(moveDefinition.Promotion);
            moveDefinition.BoardAfter = new InteractiveBoard(m_emptyBoard);
            var converter = new MoveTextConverter(moveDefinition);
            string move = converter.MoveAsText();
            Assert.AreEqual("a8=Q", move, "Pawn on a7 moves to a8 and promotes to queen"); 
        }

        [Test]
        public void AmbiguousPawnPromotes()
        {
            m_emptyBoard.FromFen("1q6/P1P5/8/8/8/8/8/k6K w - - 0");

            var moveDefinition = new BoardStateTransition
            {
                MovedPiece = m_normalBoard.GetContents(Location.A7),
                Target = Location.B8,
                Promotion = "Q",
                BoardBefore = new InteractiveBoard(m_emptyBoard)
            };

            m_emptyBoard.Move(moveDefinition.MovedPiece.Position.Location, moveDefinition.Target);
            moveDefinition.BoardAfter = new InteractiveBoard(m_emptyBoard);
            var converter = new MoveTextConverter(moveDefinition);
            string move = converter.MoveAsText();
            Assert.AreEqual("axb8=Q", move, "Pawn on a7 moves to a8 and promotes to queen");
        }

        [Test]
        public void AmbiguousPawnPromotesAndChecks()
        {
            m_emptyBoard.FromFen("1q5k/P1P5/8/8/8/8/8/7K w - - 0");

            var moveDefinition = new BoardStateTransition
            {
                MovedPiece = m_emptyBoard.GetContents(Location.A7),
                Target = Location.B8,
                Promotion = "Q",
                BoardBefore = new InteractiveBoard(m_emptyBoard)
            };

            m_emptyBoard.Move(moveDefinition.MovedPiece.Position.Location, moveDefinition.Target);
            m_emptyBoard.PromotePiece(moveDefinition.Promotion);
            moveDefinition.BoardAfter = new InteractiveBoard(m_emptyBoard);
            var converter = new MoveTextConverter(moveDefinition);
            string move = converter.MoveAsText();
            Assert.AreEqual("axb8=Q+", move, "Pawn on a7 moves to a8 and promotes to queen");
        }

        [Test]
        public void PawnPromotesAndMates()
        {
            m_emptyBoard.FromFen("k7/6PR/8/8/8/8/8/7K w - - 0");

            var moveDefinition = new BoardStateTransition
            {
                MovedPiece = m_emptyBoard.GetContents(Location.G7),
                Target = Location.G8,
                Promotion = "Q",
                BoardBefore = new InteractiveBoard(m_emptyBoard)
            };

            m_emptyBoard.Move(moveDefinition.MovedPiece.Position.Location, moveDefinition.Target);
            m_emptyBoard.PromotePiece(moveDefinition.Promotion);
            moveDefinition.BoardAfter = new InteractiveBoard(m_emptyBoard);
            var converter = new MoveTextConverter(moveDefinition);
            string move = converter.MoveAsText();
            Assert.AreEqual("g8=Q#", move, "Pawn on g7 moves to g8 and promotes to queen with mate");
        }

        [Test]
        public void NormalMoveOfKing()
        {
            m_emptyBoard.FromFen("rnbqkbnr/ppp1pppp/8/8/8/8/PPPP1PPP/RNB1KBNR w KQkq - 0");

            var moveDefinition = new BoardStateTransition
            {
                MovedPiece = m_emptyBoard.GetContents(Location.E1),
                Target = Location.D1,
                BoardBefore = new InteractiveBoard(m_emptyBoard)
            };

            m_emptyBoard.Move(moveDefinition.MovedPiece.Position.Location, moveDefinition.Target);
            moveDefinition.BoardAfter = new InteractiveBoard(m_emptyBoard);
            var converter = new MoveTextConverter(moveDefinition);
            string move = converter.MoveAsText();
            Assert.AreEqual("Kd1", move, "King moves to D1"); 
        }

        [Test]
        public void PawnTakesPiece()
        {
            m_emptyBoard.FromFen("rnbqkbnr/ppp1pppp/8/3p4/4P3/8/PPPP1PPP/RNBQKBNR w KQkq - 0");
            
            var moveDefinition = new BoardStateTransition
            {
                MovedPiece = m_emptyBoard.GetContents(Location.E4),
                Target = Location.D5,
                BoardBefore = new InteractiveBoard(m_emptyBoard)
            };

            m_emptyBoard.Move(moveDefinition.MovedPiece.Position.Location, moveDefinition.Target);
            moveDefinition.BoardAfter = new InteractiveBoard(m_emptyBoard);
            var converter = new MoveTextConverter(moveDefinition);
            string move = converter.MoveAsText();
            Assert.AreEqual("exd5", move, "Pawn move was not correctly converted to text");
        }

        [Test]
        public void PawnTakesEnPassant()
        {
            m_emptyBoard.FromFen("rnbqkbnr/pppp1ppp/8/8/P2Pp3/8/1PP1PPPP/RNBQKBNR b KQkq D3 0");

            var moveDefinition = new BoardStateTransition
            {
                MovedPiece = m_emptyBoard.GetContents(Location.E4),
                Target = Location.D3,
                BoardBefore = new InteractiveBoard(m_emptyBoard)
            };

            m_emptyBoard.Move(moveDefinition.MovedPiece.Position.Location, moveDefinition.Target);
            moveDefinition.BoardAfter = new InteractiveBoard(m_emptyBoard);
            var converter = new MoveTextConverter(moveDefinition);
            string move = converter.MoveAsText();
            Assert.AreEqual("exd3", move, "Pawn move was not correctly converted to text");
        }

        [Test]
        public void PieceMovesWithCheck()
        {
            m_emptyBoard.FromFen("rnbqkbnr/ppp1pppp/8/3b4/4Q3/8/PPPP1PPP/RNBQKBNR w KQkq - 0");

            var moveDefinition = new BoardStateTransition
            {
                MovedPiece = m_emptyBoard.GetContents(Location.E4),
                Target = Location.A4,
                BoardBefore = new InteractiveBoard(m_emptyBoard)
            };

            m_emptyBoard.Move(moveDefinition.MovedPiece.Position.Location, moveDefinition.Target);
            moveDefinition.BoardAfter = new InteractiveBoard(m_emptyBoard);
            var converter = new MoveTextConverter(moveDefinition);
            string move = converter.MoveAsText();
            Assert.AreEqual("Qa4+", move, "Queen moves to A4");
        }

        [Test]
        public void PieceMovesWithMate()
        {
            m_emptyBoard.FromFen("k7/7R/6R1/8/8/8/8/K7 w - - 0");

            var moveDefinition = new BoardStateTransition
            {
                MovedPiece = m_emptyBoard.GetContents(Location.G6),
                Target = Location.G8,
                BoardBefore = new InteractiveBoard(m_emptyBoard)
            };

            m_emptyBoard.Move(moveDefinition.MovedPiece.Position.Location, moveDefinition.Target);
            moveDefinition.BoardAfter = new InteractiveBoard(m_emptyBoard);
            var converter = new MoveTextConverter(moveDefinition);
            string move = converter.MoveAsText();
            Assert.AreEqual("Rg8#", move, "Rook moves to g8 and mate");
        }

        [Test]
        public void PieceMoves()
        {
            m_emptyBoard.FromFen("rnbqkbnr/ppp1pppp/8/3b4/4Q3/8/PPPP1PPP/RNBQKBNR w KQkq - 0");

            var moveDefinition = new BoardStateTransition
            {
                MovedPiece = m_emptyBoard.GetContents(Location.E4),
                Target = Location.B4,
                BoardBefore = new InteractiveBoard(m_emptyBoard)
            };

            m_emptyBoard.Move(moveDefinition.MovedPiece.Position.Location, moveDefinition.Target);
            moveDefinition.BoardAfter = new InteractiveBoard(m_emptyBoard);
            var converter = new MoveTextConverter(moveDefinition);
            string move = converter.MoveAsText();
            Assert.AreEqual("Qb4", move, "Queen moves to B4");
        }

        [Test]
        public void PieceTakesPiece()
        {
            m_emptyBoard.FromFen("rnbqkbnr/ppp1pppp/8/3b4/4Q3/8/PPPP1PPP/RNBQKBNR w KQkq - 0");

            var moveDefinition = new BoardStateTransition
            {
                MovedPiece = m_emptyBoard.GetContents(Location.E4),
                Target = Location.D5,
                BoardBefore = new InteractiveBoard(m_emptyBoard)
            };

            m_emptyBoard.Move(moveDefinition.MovedPiece.Position.Location, moveDefinition.Target);
            moveDefinition.BoardAfter = new InteractiveBoard(m_emptyBoard);
            var converter = new MoveTextConverter(moveDefinition);
            string move = converter.MoveAsText();
            Assert.AreEqual("Qxd5", move, "Queen takes bishop on D5");
        }

        [Test]
        public void CastleKingSide()
        {
            m_emptyBoard.FromFen("k7/8/8/8/8/8/8/4K2R w KQkq - 0");

            var moveDefinition = new BoardStateTransition
            {
                MovedPiece = m_emptyBoard.GetContents(Location.E1),
                Target = Location.G1,
                BoardBefore = new InteractiveBoard(m_emptyBoard)
            };

            m_emptyBoard.Move(moveDefinition.MovedPiece.Position.Location, moveDefinition.Target);
            moveDefinition.BoardAfter = new InteractiveBoard(m_emptyBoard);
            var converter = new MoveTextConverter(moveDefinition);
            string move = converter.MoveAsText();
            Assert.AreEqual("O-O", move, "White king castles kingside");
        }

        [Test]
        public void CastleQueenSide()
        {
            m_emptyBoard.FromFen("3k4/8/8/8/8/8/8/R3K2R w KQ - 0");

            var moveDefinition = new BoardStateTransition
            {
                MovedPiece = m_emptyBoard.GetContents(Location.E1),
                Target = Location.C1,
                BoardBefore = new InteractiveBoard(m_emptyBoard)
            };

            m_emptyBoard.Move(moveDefinition.MovedPiece.Position.Location, moveDefinition.Target);
            moveDefinition.BoardAfter = new InteractiveBoard(m_emptyBoard);
            var converter = new MoveTextConverter(moveDefinition);
            string move = converter.MoveAsText();
            Assert.AreEqual("O-O-O+", move, "White king castles queenside");
        }

        [Test]
        public void AmbiguousMoveByRow()
        {
            m_emptyBoard.FromFen("1k4K1/8/8/8/8/8/8/R6R w - - 0");

            var moveDefinition = new BoardStateTransition
            {
                MovedPiece = m_emptyBoard.GetContents(Location.A1),
                Target = Location.C1,
                BoardBefore = new InteractiveBoard(m_emptyBoard)
            };

            m_emptyBoard.Move(moveDefinition.MovedPiece.Position.Location, moveDefinition.Target);
            moveDefinition.BoardAfter = new InteractiveBoard(m_emptyBoard);
            var converter = new MoveTextConverter(moveDefinition);
            string move = converter.MoveAsText();
            Assert.AreEqual("Rac1", move, "Rook on A1 moves to C1"); 
        }

        [Test]
        public void FoolsMate()
        {
            // f3 e5
            // 2. g4 Qh4#

            m_normalBoard.Move(Location.F2, Location.F3);
            Assert.AreEqual("f3", m_normalBoard.LastMove(), "Expected pawn move f3");
            m_normalBoard.Move(Location.E7, Location.E5);
            Assert.AreEqual("e5", m_normalBoard.LastMove(), "Expected pawn move e5");
            m_normalBoard.Move(Location.G2, Location.G4);
            Assert.AreEqual("g4", m_normalBoard.LastMove(), "Expected pawn move g4");
            m_normalBoard.Move(Location.D8, Location.H4);

            Assert.AreEqual("Qh4#",m_normalBoard.LastMove(), "Expected mate with queen");
        }

        [Test]
        public void AmbiguousMoveByColumn()
        {
            m_emptyBoard.FromFen("R7/8/8/7k/8/7K/8/R7 w - - 0");

            var moveDefinition = new BoardStateTransition
            {
                MovedPiece = m_emptyBoard.GetContents(Location.A1),
                Target = Location.A3,
                BoardBefore = new InteractiveBoard(m_emptyBoard)
            };

            m_emptyBoard.Move(moveDefinition.MovedPiece.Position.Location, moveDefinition.Target);
            moveDefinition.BoardAfter = new InteractiveBoard(m_emptyBoard);

            var converter = new MoveTextConverter(moveDefinition);
            string move = converter.MoveAsText();
            Assert.AreEqual("R1a3", move, "Rook on A1 moves to a3");
        }
    }
}
