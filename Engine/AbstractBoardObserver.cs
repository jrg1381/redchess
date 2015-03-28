using System;
using Redchess.Engine.Interfaces;

namespace Redchess.Engine
{
    internal abstract class AbstractBoardObserver2<T> : IObserver<IBoardExtended>, IDisposable
    {
        protected readonly IBoardExtended Board;
        private readonly IDisposable m_unsubscriber;
        protected bool DataIsCurrent;
        protected T m_data;

        protected abstract void UpdateValue();

        public T Value
        {
            get
            {
                if (DataIsCurrent)
                    return m_data;

                UpdateValue();
                DataIsCurrent = true;

                return m_data;
            }

            protected set 
            { 
                m_data = value;
                DataIsCurrent = true;
            }
        }

        protected AbstractBoardObserver2(IBoardExtended board)
        {
            Board = board;
            m_unsubscriber = Board.Subscribe(this);
        }

        public virtual void OnError(Exception error)
        {
        }

        public virtual void OnNext(IBoardExtended value)
        {
        }

        public virtual void OnCompleted()
        {
            DataIsCurrent = false;
        }

        public virtual void Dispose()
        {
            m_unsubscriber.Dispose();
        }
    }

    internal abstract class AbstractBoardObserver : IObserver<IBoardExtended>, IDisposable
    {
        protected readonly IBoardExtended Board;
        private readonly IDisposable m_unsubscriber;
        protected bool DataIsCurrent;

        protected AbstractBoardObserver(IBoardExtended board)
        {
            Board = board;
            m_unsubscriber = Board.Subscribe(this);
        }

        public virtual void OnError(Exception error)
        {
        }

        public virtual void OnNext(IBoardExtended value)
        {
        }

        public virtual void OnCompleted()
        {
            DataIsCurrent = true;
        }

        public virtual void Dispose()
        {
            m_unsubscriber.Dispose();
        }
    }
}