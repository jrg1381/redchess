using System;
using Redchess.Engine.Interfaces;

namespace Redchess.Engine
{
    internal sealed class MoveTranscriber : IObserver<IBoardExtended>, IDisposable
    {
        private readonly IBoardExtended m_board;
        private readonly IDisposable m_unsubscriber;
        private readonly MoveTextConverter m_converter;
        private string m_lastMove;
        private BoardWithNextMove m_previousState;
        private bool m_upToDate;

        internal MoveTranscriber(IBoardExtended board)
        {
            m_board = board;
            m_unsubscriber = m_board.Subscribe(this);
            m_converter = new MoveTextConverter(board);
        }

        public string LastMove()
        {
            if(!m_upToDate)
                m_lastMove = m_converter.MoveAsText(m_previousState);

            m_upToDate = true;

            return m_lastMove;
        }

        private void UpdateMove()
        {
            m_previousState = m_board.PreviousState;
            m_upToDate = false;
        }

        public void OnCompleted()
        {
            UpdateMove();
        }

        public void OnError(Exception error)
        {
            return;
        }

        public void OnNext(IBoardExtended value)
        {
            return;
        }

        public void Dispose()
        {
            m_unsubscriber.Dispose();
        }
    }
}