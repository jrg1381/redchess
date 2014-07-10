using System;
using NUnit.Framework;
using RedChess.ChessCommon.Enumerations;
using Redchess.Engine;

namespace Redchess.EngineTests
{
    static class FenAssert
    {
        public static void AreEqual(string expectedFen, string actualFen, string userMessage)
        {
            if (expectedFen == actualFen)
                return;

            var expected = new InteractiveBoard(PieceColor.White, true);
            expected.FromFen(expectedFen);
            var actual = new InteractiveBoard(PieceColor.White, true);
            actual.FromFen(actualFen);

            string message = String.Format("{0}\nExpected:\n{1}\nActual:\n{2}", userMessage, expected, actual);
            Assert.Fail(message);
        }
    }
}