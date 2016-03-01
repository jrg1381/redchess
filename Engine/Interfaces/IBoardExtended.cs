using System;
using Redchess.Engine.Structures;
using RedChess.ChessCommon.Enumerations;
using RedChess.ChessCommon.Interfaces;

namespace Redchess.Engine.Interfaces
{
    public interface IBoardExtended : IBoard, IObservable<IBoardExtended>
    {
        bool ValidateMoveForCheck(IPiece piece, Location newLocation);
        IPiece GetContents(Location loc);
        bool MayCastle(IPiece king, Side side);
        IBoardBitmap Pieces(PieceColor color);
        Location EnPassantTarget { get; }
        int FiftyMoveCounter { get; }
        BoardStateTransition PreviousState { get; }
        CastlingOptions PermanentCastlingOptions { get; }
        bool Check { get; }
        string FenCastleString();
    }
}