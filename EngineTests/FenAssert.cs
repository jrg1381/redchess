using System;
using NUnit.Framework;
using RedChess.ChessCommon.Enumerations;

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

            string message =
                $"{userMessage}\nExpected:\n{expected}\nActual:\n{actual}\nExpected: {expectedFen}\nActual:   {actualFen}";
            Assert.Fail(message);
        }
    }
}