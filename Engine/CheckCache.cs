using System;
using System.Linq;
using RedChess.ChessCommon.Enumerations;
using Redchess.Engine.Interfaces;

namespace Redchess.Engine
{
    internal class CheckCache : AbstractBoardObserver
    {
        private bool m_isInCheck;
        private bool m_otherPlayerInCheck;
        private bool m_otherPlayerInCheckDataUpToDate;

        public CheckCache(IBoardExtended board) : base(board) {}

        public bool IsInCheck
        {
            get
            {
                if (DataIsCurrent)
                    return m_isInCheck;

                m_isInCheck = Board.KingInCheck(Board.CurrentTurn, KingPosition(Board.CurrentTurn));
                DataIsCurrent = true;
                return m_isInCheck;
            }
        }

        public bool OtherPlayerInCheck
        {
            get
            {
                if (m_otherPlayerInCheckDataUpToDate)
                    return m_otherPlayerInCheck;

                m_otherPlayerInCheck = Board.KingInCheck(~Board.CurrentTurn, KingPosition(~Board.CurrentTurn));
                m_otherPlayerInCheckDataUpToDate = true;
                return m_otherPlayerInCheck;
            }
        }

        public override void OnCompleted()
        {
            DataIsCurrent = false;
            m_otherPlayerInCheckDataUpToDate = false;
        }

        private Location KingPosition(PieceColor colorOfKing)
        {
            var king = colorOfKing == PieceColor.Black ? PieceType.BlackKing : PieceType.WhiteKing;
            // Crashes if there is no king with SequenceHasNoElements exception. This is deliberate, there should always be two kings.
            // Using FirstOrDefault will claim that the King is on A1, which is unhelpful.
            return Board.FindPieces(king).First();
        }
    }
}