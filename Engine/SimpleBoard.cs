using System.Collections.Generic;
using System.Linq;
using RedChess.ChessCommon.Enumerations;
using Redchess.Engine.Exceptions;
using Redchess.Engine.Interfaces;
using Redchess.Engine.Pieces;

namespace Redchess.Engine
{
    public class SimpleBoard
    {
        private IBoardBitmap m_blackPieces;
        private IBoardBitmap m_whitePieces;
        private Dictionary<Location, IPiece> m_squareContents;

        internal SimpleBoard(bool isEmpty)
        {
            m_squareContents = new Dictionary<Location, IPiece>();

            m_whitePieces = new BoardBitmap();
            m_blackPieces = new BoardBitmap();

            if (isEmpty) return;

            foreach (var pieceType in PieceData.WhitePieceTypes)
            {
                foreach (var loc in PieceData.InitialPieceConfiguration(pieceType))
                {
                    m_whitePieces.Add(loc);
                    m_squareContents[loc] = PieceFactory.CreatePiece(pieceType, loc);
                }
            }

            foreach (var pieceType in PieceData.BlackPieceTypes)
            {
                foreach (var loc in PieceData.InitialPieceConfiguration(pieceType))
                {
                    m_blackPieces.Add(loc);
                    m_squareContents[loc] = PieceFactory.CreatePiece(pieceType, loc);                   
                }
            }
        }

        internal IPiece GetContents(Location loc)
        {
            IPiece piece;
            bool success = m_squareContents.TryGetValue(loc, out piece);
            return success ? piece : null;
        }

        internal void AddPiece(IPiece piece)
        {
            var ownPieces = Pieces(piece.Color);

            if (ownPieces.OccupiedSquares().Contains(piece.Position.Location))
            {
                throw new DoubleOccupancyException("Two pieces of the same color cannot occupy " +
                                                   piece.Position.Location);
            }
            
            if(piece.Color == PieceColor.White)
                m_whitePieces.Add(piece.Position.Location);
            else
                m_blackPieces.Add(piece.Position.Location);

            m_squareContents[piece.Position.Location] = PieceFactory.CreatePiece(piece.Type, piece.Position.Location);
        }

        internal void RemovePiece(IPiece piece)
        {
            if (piece == null)
                return;

            if (piece.Color == PieceColor.White)
                m_whitePieces.Remove(piece.Position.Location);
            else
                m_blackPieces.Remove(piece.Position.Location);

            m_squareContents.Remove(piece.Position.Location);
        }

        internal IEnumerable<Location> OccupiedSquares()
        {
            return Pieces(PieceColor.Black).OccupiedSquares().Concat(Pieces(PieceColor.White).OccupiedSquares());
        }

        internal int PiecesOfType(PieceType t)
        {
            return OccupiedSquares().Count(x => GetContents(x).Type == t);
        }

        internal SimpleBoard DeepClone()
        {
            var copy = new SimpleBoard(true)
            {
                m_whitePieces = m_whitePieces.DeepClone(),
                m_blackPieces = m_blackPieces.DeepClone(),
                m_squareContents = new Dictionary<Location, IPiece>(m_squareContents)
            };

            return copy;
        }

        internal IBoardBitmap Pieces(PieceColor color)
        {
            return color == PieceColor.White ? m_whitePieces : m_blackPieces;
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
    }
}