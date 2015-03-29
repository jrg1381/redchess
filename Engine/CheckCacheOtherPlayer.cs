using System;
using System.Linq;
using RedChess.ChessCommon.Enumerations;
using Redchess.Engine.Interfaces;

namespace Redchess.Engine
{
    internal class CheckCacheOtherPlayer : AbstractBoardObserver2<bool>
    {
        public CheckCacheOtherPlayer(IBoardExtended board) : base(board)
        {
        }

        protected override void UpdateValue()
        {
            m_data = Board.KingInCheck(~Board.CurrentTurn, KingPosition(~Board.CurrentTurn));
        }
    }
}