using System;
using System.Collections.Generic;
using System.Security.Policy;
using System.Threading.Tasks;
using NUnit.Framework;
using Redchess.AnalysisWorker;
using RedChess.ChessCommon;
using RedChess.ChessCommon.Enumerations;
using Rhino.Mocks;

namespace Redchess.AnalysisEngineTests
{
    [TestFixture]
    public class FakeEngineTests
    {
        private UciEngineFarm m_engineWrapper;
        private List<IUciEngine> m_fakeEngines;
        private HashSet<int> m_engineCreationCount;

        private IUciEngine FakeEngineForGame(int gameId, BoardAnalysis expectedBoardAnalysis)
        {
            var fakeEngine = MockRepository.GenerateMock<IUciEngine>();
            fakeEngine.Stub(x => x.GameId).Return(gameId);
            fakeEngine.Expect(x => x.Evaluate(Arg<WorkItem>.Is.Anything)).WhenCalled(mi =>
            {
                var workItem = (mi.Arguments[0] as WorkItem);
                /* This is a device to allow us to make sure that the returned results match the 
                 * request. Instead of a FEN position, the caller specifies a guid (or other 
                 * string), and then gets it echoed back to them */
                var answer = new BoardAnalysis
                {
                    Analysis = expectedBoardAnalysis.Analysis + ":" + workItem.Fen,
                    BoardEvaluation = expectedBoardAnalysis.BoardEvaluation,
                    BoardEvaluationType = expectedBoardAnalysis.BoardEvaluationType
                };

                workItem.Result = answer;
            });

            return fakeEngine;
        }

        [SetUp]
        public void SetupFakeEngine()
        {
            m_engineCreationCount = new HashSet<int>();
            m_fakeEngines = new List<IUciEngine>();

            var answer = new BoardAnalysis
            {
                Analysis = "some stuff from stockfish",
                BoardEvaluation = -1,
                BoardEvaluationType = EvaluationType.MateInN
            };

            Func<int, IUciEngine> engineCreator = i =>
            {
                if (m_engineCreationCount.Contains(i))
                {
                    throw new InvalidOperationException("Should not create a new engine");
                }

                m_engineCreationCount.Add(i);
                var fake = FakeEngineForGame(i, answer);
                m_fakeEngines.Add(fake);
                return fake;
            };

            m_engineWrapper = new UciEngineFarm(engineCreator);
        }

        [Test]
        public void WorkerCreatedAndEvaluateCalled()
        {
            var bestmove = m_engineWrapper.EvaluateMove(10, "rnbqkbnr/pppp1ppp/8/4p3/6P1/5P2/PPPPP2P/RNBQKBNR b KQkq - 0 2", "d8h4");
            foreach (var fake in m_fakeEngines)
            {
                fake.VerifyAllExpectations();
            }
            Assert.AreEqual(-1, bestmove.BoardEvaluation, "Expected bestmove to detect mate in 1 for black");
            Assert.AreEqual(EvaluationType.MateInN, bestmove.BoardEvaluationType, "Expected bestmove to detect mate in 1 for black");
        }

        [Test]
        public void MultipleMovesInSameGameOnlyGeneratesOneWorker()
        {
            m_engineWrapper.EvaluateMove(10, "rnbqkbnr/pppp1ppp/8/4p3/6P1/5P2/PPPPP2P/RNBQKBNR b KQkq - 0 2", "d8h4");
            m_engineWrapper.EvaluateMove(10, "rnbqkbnr/pppp1ppp/8/4p3/6P1/5P2/PPPPP2P/RNBQKBNR b KQkq - 0 2", "d8h4");
            foreach (var fake in m_fakeEngines)
            {
                fake.VerifyAllExpectations();
            }
            Assert.AreEqual(1, m_engineCreationCount.Count);
        }

        [Test]
        public void MultipleMovesInNGamesGeneratesNWorkers()
        {
            m_engineWrapper.EvaluateMove(10, "rnbqkbnr/pppp1ppp/8/4p3/6P1/5P2/PPPPP2P/RNBQKBNR b KQkq - 0 2", "d8h4");
            m_engineWrapper.EvaluateMove(11, "rnbqkbnr/pppp1ppp/8/4p3/6P1/5P2/PPPPP2P/RNBQKBNR b KQkq - 0 2", "d8h4");
            foreach (var fake in m_fakeEngines)
            {
                fake.VerifyAllExpectations();
            }

            Assert.AreEqual(2, m_engineCreationCount.Count);
        }

        [Test]
        public void AsynchronousRequestsSucceed()
        {
            const int gameStart = 100;
            const int gameCount = 10;
            var tasks = new List<Task>();

            for (var gameId = gameStart; gameId < gameStart + gameCount; gameId++)
            {
                for (var repeatCount = 1; repeatCount < (gameId%5); repeatCount++)
                {
                    var id = gameId;
                    tasks.Add(Task.Run(() =>
                    {
                        var guid = Guid.NewGuid().ToString(); // use in place of fen, and make sure we get it back
                        var bestmove = m_engineWrapper.EvaluateMove(id, guid, "d8h4");
                        StringAssert.EndsWith(":" + guid, bestmove.Analysis);
                    }));
                }
            }

            Task.WaitAll(tasks.ToArray());

            foreach (var fake in m_fakeEngines)
            {
                fake.VerifyAllExpectations();
            }
        }
    }
}