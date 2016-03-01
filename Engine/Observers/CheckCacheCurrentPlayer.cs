using Redchess.Engine.Interfaces;

namespace Redchess.Engine.Observers
{
    internal class CheckCacheCurrentPlayer : AbstractBoardObserver<bool>
    {
        public CheckCacheCurrentPlayer(IBoardExtended board) : base(board)
        {
        }

        protected override void UpdateValue()
        {
            m_data = new CheckTester(Board.CurrentTurn, KingPosition(Board.CurrentTurn), Board).Check();
        }
    }
}