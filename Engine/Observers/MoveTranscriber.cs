using Redchess.Engine.Interfaces;

namespace Redchess.Engine.Observers
{
    internal sealed class MoveTranscriber : AbstractBoardObserver<string>
    {
        private BoardStateTransition m_PreviousState;

        internal MoveTranscriber(IBoardExtended board) : base(board)
        {
        }

        protected override void UpdateValue()
        {
            Value = new MoveTextConverter(m_PreviousState).MoveAsText();
        }

        public override void OnCompleted()
        {
            m_PreviousState = Board.PreviousState;
            base.OnCompleted();
        }
    }
}