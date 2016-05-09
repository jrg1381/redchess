using System.Collections.Generic;
using System.Linq;
using RedChess.ChessCommon;
using RedChess.ChessCommon.Enumerations;
using Redchess.Engine.Exceptions;
using Redchess.Engine.Interfaces;
using Redchess.Engine.Pieces;

namespace Redchess.Engine
{
    public sealed class SimpleBoard
    {
        private IBoardBitmap m_BlackPieces;
        private IBoardBitmap m_WhitePieces;
        private IPiece [] m_SquareContents;

        internal SimpleBoard(bool isEmpty)
        {
            m_SquareContents = new IPiece[64];

            m_WhitePieces = new BoardBitmap();
            m_BlackPieces = new BoardBitmap();

            if (isEmpty) return;

            foreach (var pieceType in PieceData.WhitePieceTypes)
            {
                foreach (var loc in PieceData.InitialPieceConfiguration(pieceType))
                {
                    m_WhitePieces.Add(loc);
                    m_SquareContents[(int)loc] = PieceFactory.CreatePiece(pieceType, loc);
                }
            }

            foreach (var pieceType in PieceData.BlackPieceTypes)
            {
                foreach (var loc in PieceData.InitialPieceConfiguration(pieceType))
                {
                    m_BlackPieces.Add(loc);
                    m_SquareContents[(int)loc] = PieceFactory.CreatePiece(pieceType, loc);
                }
            }
        }

        internal IPiece GetContents(Location loc)
        {
            return m_SquareContents[(int)loc];
        }

        private void AddPiece(IPiece piece)
        {
            var originalOccupant = GetContents(piece.Position.Location);
            if (originalOccupant != null && originalOccupant.Color == piece.Color)
            {
                throw new DoubleOccupancyException("Two pieces of the same color cannot occupy " +
                                                   piece.Position.Location);
            }
            
            if(piece.Color == PieceColor.White)
                m_WhitePieces.Add(piece.Position.Location);
            else
                m_BlackPieces.Add(piece.Position.Location);

            m_SquareContents[(int)piece.Position.Location] = piece;
        }

        internal void RemovePiece(IPiece piece)
        {
            if (piece == null)
                return;

            if (piece.Color == PieceColor.White)
                m_WhitePieces.Remove(piece.Position.Location);
            else
                m_BlackPieces.Remove(piece.Position.Location);

            m_SquareContents[(int)piece.Position.Location] = null;
        }

        internal IEnumerable<Location> OccupiedSquares()
        {
            return Pieces(PieceColor.Black).OccupiedSquares().Concat(Pieces(PieceColor.White).OccupiedSquares());
        }

        private int PiecesOfType(PieceType t)
        {
            return OccupiedSquares().Count(x => GetContents(x).Type.IsOfType(t));
        }

        internal SimpleBoard DeepClone()
        {
            var copy = new SimpleBoard(true)
            {
                m_WhitePieces = m_WhitePieces.DeepClone(),
                m_BlackPieces = m_BlackPieces.DeepClone(),
                m_SquareContents = (IPiece[])m_SquareContents.Clone()
            };

            return copy;
        }

        internal IBoardBitmap Pieces(PieceColor color)
        {
            return color == PieceColor.White ? m_WhitePieces : m_BlackPieces;
        }

        /// <summary>
        ///     Moves the piece with no checks on validity.
        /// </summary>
        /// <param name="piece"></param>
        /// <param name="newLocation"></param>
        internal void UnsafeMovePiece(ref IPiece piece, Location newLocation)
        {
            RemovePiece(piece);
            piece = PieceFactory.CreatePiece(piece.Type, newLocation);
            AddPiece(piece);
        }

        internal void AddPiece(PieceType pieceType, Location location)
        {
            AddPiece(PieceFactory.CreatePiece(pieceType, location));
        }

        internal bool IsDraw()
        {
            var totalPieceCount = m_WhitePieces.PieceCount + m_BlackPieces.PieceCount;
            if (totalPieceCount > 3) return false; // There are enough pieces for it not to be an obvious draw

            if (totalPieceCount == 2)
            {
                return true; // King vs King
            }

            if (PiecesOfType(PieceType.Knight) == 1 || PiecesOfType(PieceType.Bishop) == 1)
                return true;

            return false;
        }
    }
}