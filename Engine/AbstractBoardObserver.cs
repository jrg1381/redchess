using System;
using System.Linq;
using RedChess.ChessCommon.Enumerations;
using Redchess.Engine.Interfaces;

namespace Redchess.Engine
{
    internal abstract class AbstractBoardObserver<T> : IObserver<IBoardExtended>, IDisposable
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
            DataIsCurrent = false;
        }

        public virtual void Dispose()
        {
            m_unsubscriber.Dispose();
        }

        protected Location KingPosition(PieceColor colorOfKing)
        {
            var king = colorOfKing == PieceColor.Black ? PieceType.BlackKing : PieceType.WhiteKing;
            // Crashes if there is no king with SequenceHasNoElements exception. This is deliberate, there should always be two kings.
            // Using FirstOrDefault will claim that the King is on A1, which is unhelpful.
            return Board.FindPieces(king).First();
        }
    }
}