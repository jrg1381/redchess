using Redchess.Engine.Interfaces;
using RedChess.ChessCommon;
using RedChess.ChessCommon.Enumerations;

namespace Redchess.Engine
{
    internal class FiftyMoveRuleCounter : AbstractBoardObserver
    {
        public FiftyMoveRuleCounter(IBoardExtended board) : base(board)
        {
        }

        internal int HalfMoveClock { get; set; }

        public override void OnCompleted()
        {
            if (Board.PreviousState.MovedPiece.Type.IsOfType(PieceType.Pawn))
                HalfMoveClock = -1;
            if (Board.PreviousState.Board.GetContents(Board.PreviousState.Target) != null)
                HalfMoveClock = -1;

            HalfMoveClock++;
        }
    }
}