using System.Linq;
using RedChess.ChessCommon.Enumerations;
using Redchess.Engine.Interfaces;
using Redchess.Engine.Pieces;

namespace Redchess.Engine
{
    internal sealed class CheckTester
    {
        private readonly PieceColor m_colorOfKing;
        private readonly Location m_kingPosition;
        private readonly IBoardExtended m_board;
        private readonly PieceType m_opponentQueen;

        internal CheckTester(PieceColor colorOfKing, Location kingPosition, IBoardExtended board)
        {
            m_board = board;
            m_kingPosition = kingPosition;
            m_colorOfKing = colorOfKing;

            m_opponentQueen = m_colorOfKing == PieceColor.White ? PieceType.BlackQueen : PieceType.WhiteQueen;
        }

        internal bool Check()
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
                .Any(p => p?.Type == opponentKing);
        }

        private bool PieceOnKingSquareCanTakeIdenticalOpponentPiece(IPiece fakePiece, PieceType opponentPieceType)
        {
            return fakePiece.ReachableSquares(m_board)
                .Select(m_board.GetContents)
                .Any(p => p?.Type == opponentPieceType);
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
            PieceType fakePawn, opponentPawn;

            if (m_colorOfKing == PieceColor.White)
            {
                fakePawn = PieceType.WhitePawn;
                opponentPawn = PieceType.BlackPawn;
            }
            else
            {
                fakePawn = PieceType.BlackPawn;
                opponentPawn = PieceType.WhitePawn;
            }

            var pawn = PieceFactory.CreatePiece(fakePawn, m_kingPosition);
            return PieceOnKingSquareCanTakeIdenticalOpponentPiece(pawn, opponentPawn);
        }
    }
}
