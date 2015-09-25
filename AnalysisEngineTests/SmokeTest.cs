using System.Diagnostics;
using NUnit.Framework;
using System;
using Redchess.AnalysisWorker;

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
        public void LaunchStockfish2()
        {
            var bestmove = m_engineWrapper.EvaluateMove(10, "r1k4r/p2nb1p1/2b4p/1p1n1p2/2PP4/3Q1NB1/1P3PPP/R5K1 b - c3 0 19", "d5c7");
            //Assert.AreEqual("d5c7", bestmove, "Expected bestmove incorrect");
            var bestmove2 = m_engineWrapper.EvaluateMove(10, "r1k4r/p2nb1p1/2b4p/1p1n1p2/2PP4/3Q1NB1/1P3PPP/R5K1 b - c3 0 19", "d5c7");
            m_engineWrapper.GameOver(10);
            //Assert.AreEqual("d5c7", bestmove2, "Expected bestmove incorrect");
            var bestmove3 = m_engineWrapper.EvaluateMove(23, "r1k4r/p2nb1p1/2b4p/1p1n1p2/2PP4/3Q1NB1/1P3PPP/R5K1 b - c3 0 19", "d5c7");
            //Assert.AreEqual("d5c7", bestmove, "Expected bestmove incorrect");
            var bestmove4 = m_engineWrapper.EvaluateMove(23, "r1k4r/p2nb1p1/2b4p/1p1n1p2/2PP4/3Q1NB1/1P3PPP/R5K1 b - c3 0 19", "d5c7");
        }


    }
}
