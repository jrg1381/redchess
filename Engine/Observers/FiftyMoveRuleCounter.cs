using Redchess.Engine.Interfaces;
using RedChess.ChessCommon;
using RedChess.ChessCommon.Enumerations;

namespace Redchess.Engine.Observers
{
    internal class FiftyMoveRuleCounter : AbstractBoardObserver<int>
    {
        public FiftyMoveRuleCounter(IBoardExtended board) : base(board)
        {
            Value = 0;
        }

        public FiftyMoveRuleCounter(Board board, Board replacementBoard) : base(board)
        {
            Value = replacementBoard.FiftyMoveCounter;
        }

        protected override void UpdateValue()
        {
            if (Board.PreviousState == null)
                return;

            if (Board.PreviousState.MovedPiece.Type.IsOfType(PieceType.Pawn))
                Value = -1;
            if (Board.PreviousState.BoardBefore.GetContents(Board.PreviousState.Target) != null)
                Value = -1;

            Data++;
        }
    }
}