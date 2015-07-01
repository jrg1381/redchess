using Redchess.Engine.Interfaces;

namespace Redchess.Engine.Observers
{
    internal class CheckCacheOpposingPlayer : AbstractBoardObserver<bool>
    {
        public CheckCacheOpposingPlayer(IBoardExtended board) : base(board)
        {
        }

        protected override void UpdateValue()
        {
            m_data = Board.KingInCheck(~Board.CurrentTurn, KingPosition(~Board.CurrentTurn));
        }
    }
}