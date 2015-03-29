using Redchess.Engine.Interfaces;

namespace Redchess.Engine
{
    internal class CheckCacheCurrentPlayer : AbstractBoardObserver2<bool>
    {
        public CheckCacheCurrentPlayer(IBoardExtended board) : base(board)
        {
        }

        protected override void UpdateValue()
        {
            m_data = Board.KingInCheck(Board.CurrentTurn, KingPosition(Board.CurrentTurn));
        }
    }
}