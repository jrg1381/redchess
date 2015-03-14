using System;
using System.Linq;
using RedChess.ChessCommon.Enumerations;
using Redchess.Engine.Interfaces;

namespace Redchess.Engine
{
    internal class CheckCache : AbstractBoardObserver
    {
        public CheckCache(IBoardExtended board) : base(board)
        {
            try
            {
                IsInCheck = Board.KingInCheck(Board.CurrentTurn, KingPosition(Board.CurrentTurn));
                OtherPlayerInCheck = Board.KingInCheck(~Board.CurrentTurn, KingPosition(~Board.CurrentTurn));
            }
            catch (InvalidOperationException)
            {
                // TODO: For tests which use an empty board, Sequence contains no elements when looking for the king
            }
        }

        public bool IsInCheck { get; private set; }
        public bool OtherPlayerInCheck { get; private set; }

        public override void OnCompleted()
        {
            IsInCheck =  Board.KingInCheck(Board.CurrentTurn, KingPosition(Board.CurrentTurn));
            OtherPlayerInCheck = Board.KingInCheck(~Board.CurrentTurn, KingPosition(~Board.CurrentTurn));
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