using System.Linq;
using RedChess.ChessCommon;
using RedChess.ChessCommon.Enumerations;
using Redchess.Engine.Interfaces;
using Redchess.Engine.Pieces;
using Redchess.Engine.Structures;

namespace Redchess.Engine
{
    internal class CheckTester
    {
        private readonly PieceColor m_colorOfKing;
        private readonly Location m_kingPosition;
        private readonly IBoardExtended m_board;

        public CheckTester(PieceColor colorOfKing, Location kingPosition, IBoardExtended board)
        {
            m_board = board;
            m_kingPosition = kingPosition;
            m_colorOfKing = colorOfKing;
        }

        public bool Check()
        {
            if (CheckedByRankOrFile()) return true;
            if (CheckedDiagonally()) return true;
            if (CheckedByKnights()) return true;
            if (CheckedByPawns()) return true;

            return false;
        }

        private bool CheckedByKnights()
        {
            var fakeKnight = m_colorOfKing == PieceColor.White ? PieceType.WhiteKnight : PieceType.BlackKnight;
            var opponentKnight = m_colorOfKing == PieceColor.White ? PieceType.BlackKnight : PieceType.WhiteKnight;

            var knight = PieceFactory.CreatePiece(fakeKnight, m_kingPosition);
            if (knight.ReachableSquares(m_board).Select(m_board.GetContents).Any(p => p != null && p.Type == opponentKnight))
                return true;
            return false;
        }

        private bool CheckedByRankOrFile()
        {
            var fakeRook = m_colorOfKing == PieceColor.White ? PieceType.WhiteRook : PieceType.BlackRook;
            var opponentRook = m_colorOfKing == PieceColor.White ? PieceType.BlackRook : PieceType.WhiteRook;
            var opponentQueen = m_colorOfKing == PieceColor.White ? PieceType.BlackQueen : PieceType.WhiteQueen;

            var rook = PieceFactory.CreatePiece(fakeRook, m_kingPosition);
            if (
                rook.ReachableSquares(m_board)
                    .Select(m_board.GetContents)
                    .Any(p => p != null && (p.Type.IsOfType(opponentRook) || p.Type.IsOfType(opponentQueen))))
                return true;
            return false;
        }

        private bool CheckedDiagonally()
        {
            var fakeBishop = m_colorOfKing == PieceColor.White ? PieceType.WhiteBishop : PieceType.BlackBishop;
            var opponentBishop = m_colorOfKing == PieceColor.White ? PieceType.BlackBishop : PieceType.WhiteBishop;
            var opponentQueen = m_colorOfKing == PieceColor.White ? PieceType.BlackQueen : PieceType.WhiteQueen;

            var bishop = PieceFactory.CreatePiece(fakeBishop, m_kingPosition);
            if (
                bishop.ReachableSquares(m_board)
                    .Select(m_board.GetContents)
                    .Any(p => p != null && (p.Type.IsOfType(opponentBishop) || p.Type.IsOfType(opponentQueen))))
                return true;
            return false;
        }

        private bool CheckedByPawns()
        {
            var kingSquare = new Square(m_kingPosition);
            var upstream = m_colorOfKing == PieceColor.White ? 1 : -1;

            if ((kingSquare.Y + upstream) < 7 && (kingSquare.Y + upstream) > 0)
            {
                var opponentPawn = m_colorOfKing == PieceColor.White ? PieceType.BlackPawn : PieceType.WhitePawn;

                if (kingSquare.X > 0)
                {
                    var leftAttackerSquare = new Square(kingSquare.X - 1, kingSquare.Y + upstream);
                    var leftPawn = m_board.GetContents(leftAttackerSquare.Location);
                    if (leftPawn != null && leftPawn.Type.IsOfType(opponentPawn))
                        return true;
                }
                if (kingSquare.X < 7)
                {
                    var rightAttackerSquare = new Square(kingSquare.X + 1, kingSquare.Y + upstream);

                    var rightPawn = m_board.GetContents(rightAttackerSquare.Location);
                    if (rightPawn != null && rightPawn.Type.IsOfType(opponentPawn))
                        return true;
                }
            }
            return false;
        }
    }
}
