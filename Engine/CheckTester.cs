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
        private readonly PieceType m_opponentQueen;

        public CheckTester(PieceColor colorOfKing, Location kingPosition, IBoardExtended board)
        {
            m_board = board;
            m_kingPosition = kingPosition;
            m_colorOfKing = colorOfKing;

            m_opponentQueen = m_colorOfKing == PieceColor.White ? PieceType.BlackQueen : PieceType.WhiteQueen;
        }

        public bool Check()
        {
            if (CheckedByRankOrFile()) return true;
            if (CheckedDiagonally()) return true;
            if (CheckedByKnights()) return true;
            if (CheckedByPawns()) return true;
            if (CheckedByKing()) return true;

            return false;
        }

        private bool CheckedByKing()
        {
            PieceType fakeKing, opponentKing;

            if (m_colorOfKing == PieceColor.White)
            {
                fakeKing = PieceType.WhiteKing;
                opponentKing = PieceType.BlackKing;
            }
            else
            {
                fakeKing = PieceType.BlackKing;
                opponentKing = PieceType.WhiteKing;
            }

            var king = PieceFactory.CreatePiece(fakeKing, m_kingPosition);
            // We need to use attacked squares because ReachableSquares includes squares reachable by castling
            return king.AttackedSquares(m_board)
                .Select(m_board.GetContents)
                .Any(p => p != null && p.Type == opponentKing);
        }

        private bool PieceOnKingSquareCanTakeIdenticalOpponentPiece(IPiece fakePiece, PieceType opponentPieceType)
        {
            return fakePiece.ReachableSquares(m_board)
                .Select(m_board.GetContents)
                .Any(p => p != null && p.Type == opponentPieceType);
        }

        private bool PieceOnKingSquareCanTakeIdenticalOpponentPieceOrQueen(IPiece fakePiece, PieceType opponentPieceType)
        {
            return fakePiece.ReachableSquares(m_board)
                .Select(m_board.GetContents)
                .Any(p => p != null && (p.Type == opponentPieceType || p.Type == m_opponentQueen));
        }

        private bool CheckedByKnights()
        {
            PieceType fakeKnight, opponentKnight;

            if (m_colorOfKing == PieceColor.White)
            {
                fakeKnight = PieceType.WhiteKnight;
                opponentKnight = PieceType.BlackKnight;
            }
            else
            {
                fakeKnight = PieceType.BlackKnight;
                opponentKnight = PieceType.WhiteKnight;
            }

            var knight = PieceFactory.CreatePiece(fakeKnight, m_kingPosition);
            return PieceOnKingSquareCanTakeIdenticalOpponentPiece(knight, opponentKnight);
        }

        private bool CheckedByRankOrFile()
        {
            PieceType fakeRook, opponentRook;

            if (m_colorOfKing == PieceColor.White)
            {
                fakeRook = PieceType.WhiteRook;
                opponentRook = PieceType.BlackRook;
            }
            else
            {
                fakeRook = PieceType.BlackRook;
                opponentRook = PieceType.WhiteRook;
            }

            var rook = PieceFactory.CreatePiece(fakeRook, m_kingPosition);
            return PieceOnKingSquareCanTakeIdenticalOpponentPieceOrQueen(rook, opponentRook);
        }

        private bool CheckedDiagonally()
        {
            PieceType fakeBishop, opponentBishop;

            if (m_colorOfKing == PieceColor.White)
            {
                fakeBishop = PieceType.WhiteBishop;
                opponentBishop = PieceType.BlackBishop;
            }
            else
            {
                fakeBishop = PieceType.BlackBishop;
                opponentBishop = PieceType.WhiteBishop;
            }

            var bishop = PieceFactory.CreatePiece(fakeBishop, m_kingPosition);
            return PieceOnKingSquareCanTakeIdenticalOpponentPieceOrQueen(bishop, opponentBishop);
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
