using System;
using Redchess.Engine.Interfaces;

namespace Redchess.Engine
{
    internal sealed class MoveTranscriber : AbstractBoardObserver<string>
    {
        private BoardWithNextMove m_previousState;

        internal MoveTranscriber(IBoardExtended board) : base(board)
        {
        }

        protected override void UpdateValue()
        {
            Value = (new MoveTextConverter(m_previousState)).MoveAsText();
        }

        public override void OnCompleted()
        {
            m_previousState = Board.PreviousState;
            base.OnCompleted();
        }
    }
}