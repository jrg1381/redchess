using System.Diagnostics;
using NUnit.Framework;
using Redchess.AnalysisWorker;
using RedChess.ChessCommon.Enumerations;

namespace Redchess.AnalysisEngineTests
{
    [TestFixture]
    public class SmokeTest
    {
        private UciEngineFarm m_engineWrapper;

        [SetUp]
        public void CreateBidiProcess()
        {
            m_engineWrapper = new UciEngineFarm();
        }

        [TearDown]
        public void Teardown()
        {
            m_engineWrapper.Dispose();
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
    }
}
