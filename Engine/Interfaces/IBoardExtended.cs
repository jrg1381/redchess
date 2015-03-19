using System;
using RedChess.ChessCommon.Enumerations;
using RedChess.ChessCommon.Interfaces;

namespace Redchess.Engine.Interfaces
{
    public interface IBoardExtended : IBoard, IObservable<IBoardExtended>
    {
        bool KingInCheck(PieceColor colorOfKing, Location kingPosition);
        bool ValidateMoveForCheck(IPiece piece, Location newLocation);
        IPiece GetContents(Location loc);
        bool MayCastle(IPiece king, Side side);
        IBoardBitmap Pieces(PieceColor color);
        Location EnPassantTarget { get; }
        int FiftyMoveCounter { get; }
        BoardWithNextMove PreviousState { get; }
        string FenCastleString();
    }
}