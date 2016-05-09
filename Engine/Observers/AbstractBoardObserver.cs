using System;
using System.Linq;
using Redchess.Engine.Interfaces;
using RedChess.ChessCommon.Enumerations;

namespace Redchess.Engine.Observers
{
    abstract class AbstractBoardObserver<T> : IObserver<IBoardExtended>, IDisposable
    {
        protected readonly IBoardExtended Board;
        private readonly IDisposable m_Unsubscriber;
        protected bool DataIsCurrent;
        protected T Data;

        protected abstract void UpdateValue();

        public T Value
        {
            get
            {
                if (DataIsCurrent)
                    return Data;

                UpdateValue();
                DataIsCurrent = true;

                return Data;
            }

            protected set 
            { 
                Data = value;
                DataIsCurrent = true;
            }
        }

        protected AbstractBoardObserver(IBoardExtended board)
        {
            Board = board;
            m_Unsubscriber = Board.Subscribe(this);
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
            m_Unsubscriber.Dispose();
        }

        protected Location KingPosition(PieceColor colorOfKing)
        {
            var king = colorOfKing == PieceColor.Black ? PieceType.BlackKing : PieceType.WhiteKing;
            // Crashes if there is no king with SequenceHasNoElements exception. This is deliberate, there should always be two kings.
            // Using FirstOrDefault will claim that the King is on A1, which is unhelpful.
            return Board.FindPieces(king).First();
        }

        public void ForceUpdate(T newValue)
        {
            Data = newValue;
            DataIsCurrent = true;
        }
    }
}