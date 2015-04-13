using Redchess.Engine.Interfaces;

namespace Redchess.Engine.Observers
{
    internal class CheckCacheOtherPlayer : AbstractBoardObserver<bool>
    {
        public CheckCacheOtherPlayer(IBoardExtended board) : base(board)
        {
        }

        protected override void UpdateValue()
        {
            m_data = Board.KingInCheck(~Board.CurrentTurn, KingPosition(~Board.CurrentTurn));
        }
    }
}