using System;
using System.Collections.Generic;
using RedChess.ChessCommon.Enumerations;

namespace RedChess.ChessCommon.Interfaces
{
    public interface IBoard : IDisposable
    {
        PieceColor CurrentTurn { get; set; }
        bool IsCheckmate(bool skipCheckTest);
        string ToFen();
        void FromFen(string fen);
        bool Move(Location start, Location end);
        void PromotePiece(string pieceName);
        IEnumerable<Location> FindPieces(PieceType pieceType);
        bool IsAwaitingPromotionDecision();
        string LastMove();
        GameStatus StatusForBoard();
    }
}