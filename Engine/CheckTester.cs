using System.Linq;
using RedChess.ChessCommon.Enumerations;
using Redchess.Engine.Interfaces;
using Redchess.Engine.Pieces;

namespace Redchess.Engine
{
    internal sealed class CheckTester
    {
        private readonly PieceColor m_ColorOfKing;
        private readonly Location m_KingPosition;
        private readonly IBoardExtended m_Board;
        private readonly PieceType m_OpponentQueen;

        internal CheckTester(PieceColor colorOfKing, Location kingPosition, IBoardExtended board)
        {
            m_Board = board;
            m_KingPosition = kingPosition;
            m_ColorOfKing = colorOfKing;

            m_OpponentQueen = m_ColorOfKing == PieceColor.White ? PieceType.BlackQueen : PieceType.WhiteQueen;
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

            if (m_ColorOfKing == PieceColor.White)
            {
                fakeKing = PieceType.WhiteKing;
                opponentKing = PieceType.BlackKing;
            }
            else
            {
                fakeKing = PieceType.BlackKing;
                opponentKing = PieceType.WhiteKing;
            }

            var king = PieceFactory.CreatePiece(fakeKing, m_KingPosition);
            // We need to use attacked squares because ReachableSquares includes squares reachable by castling
            return king.AttackedSquares(m_Board)
                .Select(m_Board.GetContents)
                .Any(p => p?.Type == opponentKing);
        }

        private bool PieceOnKingSquareCanTakeIdenticalOpponentPiece(IPiece fakePiece, PieceType opponentPieceType)
        {
            return fakePiece.ReachableSquares(m_Board)
                .Select(m_Board.GetContents)
                .Any(p => p?.Type == opponentPieceType);
        }

        private bool PieceOnKingSquareCanTakeIdenticalOpponentPieceOrQueen(IPiece fakePiece, PieceType opponentPieceType)
        {
            return fakePiece.ReachableSquares(m_Board)
                .Select(m_Board.GetContents)
                .Any(p => p != null && (p.Type == opponentPieceType || p.Type == m_OpponentQueen));
        }

        private bool CheckedByKnights()
        {
            PieceType fakeKnight, opponentKnight;

            if (m_ColorOfKing == PieceColor.White)
            {
                fakeKnight = PieceType.WhiteKnight;
                opponentKnight = PieceType.BlackKnight;
            }
            else
            {
                fakeKnight = PieceType.BlackKnight;
                opponentKnight = PieceType.WhiteKnight;
            }

            var knight = PieceFactory.CreatePiece(fakeKnight, m_KingPosition);
            return PieceOnKingSquareCanTakeIdenticalOpponentPiece(knight, opponentKnight);
        }

        private bool CheckedByRankOrFile()
        {
            PieceType fakeRook, opponentRook;

            if (m_ColorOfKing == PieceColor.White)
            {
                fakeRook = PieceType.WhiteRook;
                opponentRook = PieceType.BlackRook;
            }
            else
            {
                fakeRook = PieceType.BlackRook;
                opponentRook = PieceType.WhiteRook;
            }

            var rook = PieceFactory.CreatePiece(fakeRook, m_KingPosition);
            return PieceOnKingSquareCanTakeIdenticalOpponentPieceOrQueen(rook, opponentRook);
        }

        private bool CheckedDiagonally()
        {
            PieceType fakeBishop, opponentBishop;

            if (m_ColorOfKing == PieceColor.White)
            {
                fakeBishop = PieceType.WhiteBishop;
                opponentBishop = PieceType.BlackBishop;
            }
            else
            {
                fakeBishop = PieceType.BlackBishop;
                opponentBishop = PieceType.WhiteBishop;
            }

            var bishop = PieceFactory.CreatePiece(fakeBishop, m_KingPosition);
            return PieceOnKingSquareCanTakeIdenticalOpponentPieceOrQueen(bishop, opponentBishop);
        }

        private bool CheckedByPawns()
        {
            PieceType fakePawn, opponentPawn;

            if (m_ColorOfKing == PieceColor.White)
            {
                fakePawn = PieceType.WhitePawn;
                opponentPawn = PieceType.BlackPawn;
            }
            else
            {
                fakePawn = PieceType.BlackPawn;
                opponentPawn = PieceType.WhitePawn;
            }

            var pawn = PieceFactory.CreatePiece(fakePawn, m_KingPosition);
            return PieceOnKingSquareCanTakeIdenticalOpponentPiece(pawn, opponentPawn);
        }
    }
}
