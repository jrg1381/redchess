using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using NUnit.Framework;
using Redchess.AnalysisWorker;
using RedChess.ChessCommon;
using RedChess.ChessCommon.Enumerations;
using Rhino.Mocks;
using Rhino.Mocks.Interfaces;

namespace Redchess.AnalysisEngineTests
{
    [TestFixture]
    public sealed class FakeEngineTests : IDisposable
    {
        private UciEngineFarm m_EngineWrapper;
        private List<IUciEngine> m_FakeEngines;
        private HashSet<int> m_EngineCreationCount;

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
            m_EngineCreationCount = new HashSet<int>();
            m_FakeEngines = new List<IUciEngine>();

            var answer = new BoardAnalysis
            {
                Analysis = "some stuff from stockfish",
                BoardEvaluation = -1,
                BoardEvaluationType = EvaluationType.MateInN
            };

            Func<int, IUciEngine> engineCreator = i =>
            {
                if (m_EngineCreationCount.Contains(i))
                {
                    throw new InvalidOperationException("Should not create a new engine");
                }
                m_EngineCreationCount.Add(i);

                var fake = FakeEngineForGame(i, answer);
                m_FakeEngines.Add(fake);
                return fake;
            };

            m_EngineWrapper = new UciEngineFarm(engineCreator);
        }

        [Test]
        public void WorkerCreatedAndEvaluateCalled()
        {
            var bestmove = m_EngineWrapper.EvaluateMove(10, "rnbqkbnr/pppp1ppp/8/4p3/6P1/5P2/PPPPP2P/RNBQKBNR b KQkq - 0 2", "d8h4");
            foreach (var fake in m_FakeEngines)
            {
                fake.VerifyAllExpectations();
            }
            Assert.AreEqual(-1, bestmove.BoardEvaluation, "Expected bestmove to detect mate in 1 for black");
            Assert.AreEqual(EvaluationType.MateInN, bestmove.BoardEvaluationType, "Expected bestmove to detect mate in 1 for black");
        }

        [Test]
        public void MultipleMovesInSameGameOnlyGeneratesOneWorker()
        {
            m_EngineWrapper.EvaluateMove(10, "rnbqkbnr/pppp1ppp/8/4p3/6P1/5P2/PPPPP2P/RNBQKBNR b KQkq - 0 2", "d8h4");
            m_EngineWrapper.EvaluateMove(10, "rnbqkbnr/pppp1ppp/8/4p3/6P1/5P2/PPPPP2P/RNBQKBNR b KQkq - 0 2", "d8h4");
            ExpectNoDispose(10);
            foreach (var fake in m_FakeEngines)
            {
                fake.VerifyAllExpectations();
            }
            Assert.AreEqual(1, m_EngineCreationCount.Count);
        }

        [Test]
        public void MultipleMovesInNGamesGeneratesNWorkers()
        {
            m_EngineWrapper.EvaluateMove(10, "rnbqkbnr/pppp1ppp/8/4p3/6P1/5P2/PPPPP2P/RNBQKBNR b KQkq - 0 2", "d8h4");
            ExpectNoDispose(10);
            m_EngineWrapper.EvaluateMove(11, "rnbqkbnr/pppp1ppp/8/4p3/6P1/5P2/PPPPP2P/RNBQKBNR b KQkq - 0 2", "d8h4");
            ExpectNoDispose(11);
            foreach (var fake in m_FakeEngines)
            {
                fake.VerifyAllExpectations();
            }

            Assert.AreEqual(2, m_EngineCreationCount.Count);
        }

        [Test]
        public void GameOverDisposesAndRecreatesWorker()
        {
            var signal = new object();
            m_EngineWrapper.EvaluateMove(10, "rnbqkbnr/pppp1ppp/8/4p3/6P1/5P2/PPPPP2P/RNBQKBNR b KQkq - 0 2", "d8h4");
            ExpectDispose(10, signal);
            lock (signal)
            {
                m_EngineWrapper.GameOver(10); // Expect this to call Dispose
                Monitor.Wait(signal);
            }
            m_EngineWrapper.EvaluateMove(10, "rnbqkbnr/pppp1ppp/8/4p3/6P1/5P2/PPPPP2P/RNBQKBNR b KQkq - 0 2", "d8h4");
            foreach (var fake in m_FakeEngines)
            {
                fake.VerifyAllExpectations();
            }

            Assert.AreEqual(1, m_EngineCreationCount.Count);
        }

        private void ExpectDispose(int gameId, object signal)
        {
            DisposeReplacement(gameId, signal).Repeat.Once();
        }

        private void ExpectNoDispose(int gameId)
        {
            DisposeReplacement(gameId, new object()).Repeat.Never();
        }

        private IMethodOptions<RhinoMocksExtensions.VoidType> DisposeReplacement(int gameId, object signal)
        {
            return m_FakeEngines.
                First(engine => engine.GameId == gameId).
                Expect(x => x.Dispose()).WhenCalled(mi =>
                {
                    lock (signal)
                    {
                        m_EngineCreationCount.Remove(gameId);
                        Monitor.Pulse(signal);
                    }
                });
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
                        var bestmove = m_EngineWrapper.EvaluateMove(id, guid, "d8h4");
                        StringAssert.EndsWith(":" + guid, bestmove.Analysis);
                    }));
                }
            }

            Task.WaitAll(tasks.ToArray());

            foreach (var fake in m_FakeEngines)
            {
                fake.VerifyAllExpectations();
            }
        }

        public void Dispose()
        {
            Dispose(true);
        }

        private void Dispose(bool isDisposing)
        {
            m_EngineWrapper.Dispose();
        }
    }
}