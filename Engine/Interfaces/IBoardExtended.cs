using RedChess.ChessCommon.Enumerations;
using RedChess.ChessCommon.Interfaces;

namespace Redchess.Engine.Interfaces
{
    public interface IBoardExtended : IBoard
    {
        bool KingInCheck(PieceColor colorOfKing, Location kingPosition);
        bool ValidateMoveForCheck(IPiece piece, Location newLocation);
        IPiece GetContents(Location loc);
        bool MayCastle(IPiece king, Side side);
        IBoardBitmap Pieces(PieceColor color);
        Location EnPassantTarget { get; }

        void AddPiece(IPiece piece);
    }
}