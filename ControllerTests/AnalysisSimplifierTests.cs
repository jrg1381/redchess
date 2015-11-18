using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NUnit.Framework;
using RedChess.ChessCommon;
using RedChess.ChessCommon.Enumerations;
using RedChess.WebEngine.Models;
using RedChess.WebEngine.Repositories;
using RedChess.WebEngine.Repositories.Interfaces;
using Rhino.Mocks;

namespace RedChess.ControllerTests
{
    [TestFixture]
    public class AnalysisSimplifierTests
    {
        IHistoryRepository GetFakeHistoryRepo(HistoryEntry historyEntry)
        {
            var mock = MockRepository.GenerateMock<IHistoryRepository>();

            mock.Expect(m => m.FindByGameIdAndMoveNumber(historyEntry.GameId, historyEntry.MoveNumber))
                .Return(historyEntry);

            return mock;
        }

        [Test]
        public void NonMateInNUnchanged()
        {
            var historyEntry = new HistoryEntry
            {
                GameId = 10,
                MoveNumber = 1,
                Fen = "rnbqkbnr/pppppppp/8/8/8/8/PPPPPPPP/RNBQKBNR w KQkq - 0",
                Move = "Nc3"
            };

            var mock = GetFakeHistoryRepo(historyEntry);
            var processor = new AnalysisSimplifier(mock);
            var boardAnalysis = new BoardAnalysis
            {
                BoardEvaluation = 44,
                Analysis = "doesn't matter",
                BoardEvaluationType = EvaluationType.Centipawn
            };

            var newAnalysis = processor.ProcessBoardAnalysis(10, 1, boardAnalysis);
            Assert.AreEqual(newAnalysis, boardAnalysis, "Expected analysis text to be unchanged for non-mate analysis");
            mock.VerifyAllExpectations();
        }

        [Test]
        public void MateInNModified()
        {
            var historyEntry = new HistoryEntry
            {
                GameId = 10,
                MoveNumber = 1,
                Fen = "rnbqkbnr/pppppppp/8/8/8/8/PPPPPPPP/RNBQKBNR w KQkq - 0",
                Move = "Nc3"
            };

            var mock = GetFakeHistoryRepo(historyEntry);
            var processor = new AnalysisSimplifier(mock);
            var boardAnalysis = new BoardAnalysis
            {
                BoardEvaluation = -11,
                Analysis = "score mate -11 nodes 4884896 nps 1051419 tbhits 0 time 4646 pv b1c3 b8c6 info depth 34",
                BoardEvaluationType = EvaluationType.MateInN
            };

            var expectedAnalysis = new BoardAnalysis
            {
                BoardEvaluation = -11,
                Analysis = "score mate -11 nodes 4884896 nps 1051419 tbhits 0 time 4646 pv Nc3 Nc6 info depth 34",
                BoardEvaluationType = EvaluationType.MateInN
            };
            
            var newAnalysis = processor.ProcessBoardAnalysis(10, 1, boardAnalysis);
            Assert.AreEqual(expectedAnalysis, newAnalysis, "Expected analysis text to have move substituted in");
            mock.VerifyAllExpectations();
        }
    }
}
