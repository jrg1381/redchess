using NUnit.Framework;
using RedChess.ChessCommon.Enumerations;
using Redchess.Engine;
using Redchess.Engine.Interfaces;

namespace EngineTests
{
    internal abstract class AbstractChessTest
    {
        protected InteractiveBoard m_normalBoard;
        protected InteractiveBoard m_emptyBoard;
        protected IBoardExtended m_betterBoardNormal;
        protected IBoardExtended m_betterBoardEmpty;

        [SetUp]
        public void TestSetup()
        {
            // Reset all the pieces between tests
            m_emptyBoard = new InteractiveBoard(PieceColor.White, true);
            m_normalBoard = new InteractiveBoard(PieceColor.White, false);

            m_betterBoardNormal = m_normalBoard;
            m_betterBoardEmpty = m_emptyBoard;
        }
    }
}