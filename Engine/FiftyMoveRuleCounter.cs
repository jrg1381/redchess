using Redchess.Engine.Interfaces;
using RedChess.ChessCommon;
using RedChess.ChessCommon.Enumerations;

namespace Redchess.Engine
{
    internal class FiftyMoveRuleCounter : AbstractBoardObserver<int>
    {
        public FiftyMoveRuleCounter(IBoardExtended board) : base(board)
        {
            Value = 0;
        }

        public void ForceUpdate(int moveCounter)
        {
            Value = moveCounter;
            DataIsCurrent = true;
        }

        protected override void UpdateValue()
        {
            if (Board.PreviousState == null)
                return;

            if (Board.PreviousState.MovedPiece.Type.IsOfType(PieceType.Pawn))
                Value = -1;
            if (Board.PreviousState.BoardBefore.GetContents(Board.PreviousState.Target) != null)
                Value = -1;

            m_data++;
        }
    }
}