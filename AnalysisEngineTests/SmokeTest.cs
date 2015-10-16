using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using NUnit.Framework;
using Redchess.AnalysisWorker;
using RedChess.ChessCommon.Enumerations;

namespace Redchess.AnalysisEngineTests
{
    [TestFixture]
    public class SmokeTest : IDisposable
    {
        private UciEngineFarm m_engineWrapper;

        [SetUp]
        public void CreateBidiProcess()
        {
            m_engineWrapper = new UciEngineFarm();
            try
            {
                Array.ForEach(StockfishProcesses(), p => p.Kill());
            }
            catch (Win32Exception e)
            {
                // Might get process denied
                Console.WriteLine(e);
            }
        }

        [TearDown]
        public void Teardown()
        {
            m_engineWrapper.Dispose();
        }

        [Test]
        public void NoStockfishInstancesRunningAfterDisposal()
        {
            for (int i = 1; i < 5; i++)
            {
                var bestmove = m_engineWrapper.EvaluateMove(i,
                    "rnbqkbnr/pppp1ppp/8/4p3/6P1/5P2/PPPPP2P/RNBQKBNR b KQkq - 0 2", "d8h4");
            }

            m_engineWrapper.Dispose();

            int count;
            var expiry = DateTime.UtcNow.AddSeconds(30);
            while ((count = StockfishProcesses().Length) > 0 && DateTime.UtcNow < expiry)
            {
                Thread.Sleep(250);
                Console.WriteLine("Stockfish processes : {0}", count);
            }

            CollectionAssert.IsEmpty(Process.GetProcessesByName("stockfish-6-64"), "Expected no stockfish64 processes to be running");
        }

        private static Process[] StockfishProcesses()
        {
            return Process.GetProcessesByName("stockfish-6-64");
        }

        [Test]
        public void DetectMateInOneForBlack()
        {
            var bestmove = m_engineWrapper.EvaluateMove(10, "rnbqkbnr/pppp1ppp/8/4p3/6P1/5P2/PPPPP2P/RNBQKBNR b KQkq - 0 2", "d8h4");
            Assert.AreEqual(-1, bestmove.BoardEvaluation, "Expected bestmove to detect mate in 1 for black");
            Assert.AreEqual(EvaluationType.MateInN, bestmove.BoardEvaluationType, "Expected bestmove to detect mate in 1 for black");
        }

        [Test]
        public void DetectMateInOneForWhite()
        {
            var bestmove = m_engineWrapper.EvaluateMove(10, "7k/5ppp/8/8/8/8/8/KQ6 w - - 0 2", "b1b8");
            Assert.AreEqual(1, bestmove.BoardEvaluation, "Expected bestmove to detect mate in 1 for white");
            Assert.AreEqual(EvaluationType.MateInN, bestmove.BoardEvaluationType, "Expected bestmove to detect mate in 1 for white");
        }

        [Test]
        public void DetectAdvantageForWhite()
        {
            var bestmove = m_engineWrapper.EvaluateMove(10, "7k/6q1/8/8/8/8/8/KQR4 w - - 0 2", "b1b2");
            Assert.Greater(bestmove.BoardEvaluation, 0, "Expected bestmove to detect advantage for white");
        }

        [Test]
        public void DetectAdvantageForBlack()
        {
            var bestmove = m_engineWrapper.EvaluateMove(10, "7k/5ppp/8/8/8/8/8/K7 b - - 0 2", "h7h5");
            Assert.Less(bestmove.BoardEvaluation, 0, "Expected bestmove to detect advantage for black");
        }

        [Test]
        public void PlayMultipleGames()
        {
            var bestmove1 = m_engineWrapper.EvaluateMove(10, "7k/5ppp/8/8/8/8/8/K7 b - - 0 2", "h7h5");
            Assert.Less(bestmove1.BoardEvaluation, 0, "Expected bestmove to detect advantage for black");
            var bestmove2 = m_engineWrapper.EvaluateMove(11, "7k/5ppp/8/8/8/8/8/K7 b - - 0 2", "h7h5");
            Assert.Less(bestmove2.BoardEvaluation, 0, "Expected bestmove to detect advantage for black");
            m_engineWrapper.GameOver(11);
            var bestmove3 = m_engineWrapper.EvaluateMove(12, "7k/5ppp/8/8/8/8/8/K7 b - - 0 2", "h7h5");
            Assert.Less(bestmove3.BoardEvaluation, 0, "Expected bestmove to detect advantage for black");
        }

        public void Dispose()
        {
            m_engineWrapper.Dispose();
        }
    }
}
