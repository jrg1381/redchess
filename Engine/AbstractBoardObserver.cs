using System;
using Redchess.Engine.Interfaces;

namespace Redchess.Engine
{
    internal abstract class AbstractBoardObserver : IObserver<IBoardExtended>, IDisposable
    {
        protected readonly IBoardExtended Board;
        protected readonly IDisposable Unsubscriber;

        protected AbstractBoardObserver(IBoardExtended board)
        {
            Board = board;
            Unsubscriber = Board.Subscribe(this);
        }

        public virtual void OnError(Exception error)
        {
        }

        public virtual void OnNext(IBoardExtended value)
        {
        }

        public virtual void OnCompleted()
        {
        }

        public virtual void Dispose()
        {
            Unsubscriber.Dispose();
        }
    }
}