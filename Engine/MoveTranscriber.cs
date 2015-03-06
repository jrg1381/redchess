using System;
using Redchess.Engine.Interfaces;

namespace Redchess.Engine
{
    internal sealed class MoveTranscriber : AbstractBoardObserver
    {
        private string m_lastMove;
        private BoardWithNextMove m_previousState;
        private bool m_upToDate;

        internal MoveTranscriber(IBoardExtended board) : base(board)
        {
        }

        internal string LastMove()
        {
            if(!m_upToDate)
                m_lastMove = (new MoveTextConverter(m_previousState)).MoveAsText();

            m_upToDate = true;

            return m_lastMove;
        }

        private void UpdateMove()
        {
            m_previousState = Board.PreviousState;
            m_upToDate = false;
        }

        public override void OnCompleted()
        {
            UpdateMove();
        }
    }
}