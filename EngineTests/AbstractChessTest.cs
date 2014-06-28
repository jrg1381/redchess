using NUnit.Framework;
using RedChess.ChessCommon.Enumerations;
using Redchess.Engine;
using Redchess.Engine.Interfaces;

namespace EngineTests
{
    internal abstract class AbstractChessTest
    {
        protected IBoardExtended m_normalBoard;
        protected IBoardExtended m_emptyBoard;

        [SetUp]
        public void TestSetup()
        {
            // Reset all the pieces between tests
            //m_emptyBoard = new InteractiveBoard(PieceColor.White, true);
            m_normalBoard = new InteractiveBoard(PieceColor.White, false);
            m_emptyBoard = new InteractiveBoard(PieceColor.White, true);
        }
    }
}