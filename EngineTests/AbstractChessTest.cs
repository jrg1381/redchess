using System;
using NUnit.Framework;
using RedChess.ChessCommon.Enumerations;

namespace Redchess.EngineTests
{
    internal abstract class AbstractChessTest : IDisposable
    {
        protected InteractiveBoard NormalBoard;
        protected InteractiveBoard EmptyBoard;

        [SetUp]
        public void TestSetup()
        {
            // Reset all the pieces between tests
            //m_emptyBoard = new InteractiveBoard(PieceColor.White, true);
            NormalBoard = new InteractiveBoard(PieceColor.White, false);
            EmptyBoard = new InteractiveBoard(PieceColor.White, true);
        }

        public void Dispose()
        {
            NormalBoard.Dispose();
            EmptyBoard.Dispose();
        }
    }
}