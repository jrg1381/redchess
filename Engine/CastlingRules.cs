using System;
using System.Collections.Generic;
using System.Linq;
using RedChess.ChessCommon.Enumerations;
using Redchess.Engine.Interfaces;

namespace Redchess.Engine
{
    sealed class CastlingRules
    {
        private bool m_blackMayCastleKingSide = true;
        private bool m_blackMayCastleQueenSide = true;
        private bool m_whiteMayCastleKingSide = true;
        private bool m_whiteMayCastleQueenSide = true;

        /// <summary>
        ///     Update the castling settings from a fen substring (something like "KkQq") where K indicates castling White
        ///     king-side, etc.
        /// </summary>
        /// <param name="fenSubstring"></param>
        internal void UpdateFromFen(string fenSubstring)
        {
            m_whiteMayCastleKingSide = fenSubstring.Contains("K");
            m_blackMayCastleKingSide = fenSubstring.Contains("k");
            m_whiteMayCastleQueenSide = fenSubstring.Contains("Q");
            m_blackMayCastleQueenSide = fenSubstring.Contains("q");
        }

        /// <summary>
        ///     Return a fen substring ("KkQq") representing the current castling state
        /// </summary>
        /// <returns></returns>
        internal string FenCastleString()
        {
            string whiteKingside = m_whiteMayCastleKingSide ? "K" : "";
            string whiteQueenside = m_whiteMayCastleQueenSide ? "Q" : "";
            string blackKingside = m_blackMayCastleKingSide ? "k" : "";
            string blackQueenside = m_blackMayCastleQueenSide ? "q" : "";

            string answer = String.Format("{0}{1}{2}{3}", whiteKingside, whiteQueenside, blackKingside, blackQueenside);

            return String.IsNullOrEmpty(answer) ? "-" : answer;
        }

        /// <summary>
        ///     Returns true if all the supplied squares are empty
        /// </summary>
        /// <param name="squares">Set of squares to check</param>
        /// <returns></returns>
        private bool SquaresEmpty(IEnumerable<Location> squares, IBoardExtended game)
        {
            return squares.All(s => game.GetContents(s) == null);
        }

        /// <summary>
        ///     Returns true if none of the supplied squares are attacked by the opponent's pieces
        /// </summary>
        /// <param name="color">The friendly side</param>
        /// <param name="squares">Set of squares to check</param>
        /// <returns></returns>
        private bool SquaresNotAttacked(PieceColor color, IEnumerable<Location> squares, IBoardExtended game)
        {
            return squares.All(s => !game.KingInCheck(color, s));
        }

        /// <summary>
        ///     Returns true if the
        ///     <param name="color">color</param>
        ///     player can castle on the specified side of the board
        /// </summary>
        /// <param name="color"></param>
        /// <param name="sideOfBoard"></param>
        /// <returns></returns>
        internal bool MayCastle(PieceColor color, Side sideOfBoard, IBoardExtended game)
        {
            switch (color)
            {
                case PieceColor.White:
                    switch (sideOfBoard)
                    {
                        case Side.KingSide:
                            return m_whiteMayCastleKingSide && SquaresEmpty(new[] {Location.F1, Location.G1}, game) &&
                                   SquaresNotAttacked(color, new[] {Location.E1, Location.F1, Location.G1}, game);
                        case Side.QueenSide:
                            return m_whiteMayCastleQueenSide &&
                                   SquaresEmpty(new[] {Location.B1, Location.C1, Location.D1}, game) &&
                                   SquaresNotAttacked(color, new[] {Location.C1, Location.D1, Location.E1}, game);
                    }
                    break;
                case PieceColor.Black:
                {
                    switch (sideOfBoard)
                    {
                        case Side.KingSide:
                            return m_blackMayCastleKingSide && SquaresEmpty(new[] {Location.F8, Location.G8}, game) &&
                                   SquaresNotAttacked(color, new[] {Location.E8, Location.F8, Location.G8}, game);
                        case Side.QueenSide:
                            return m_blackMayCastleQueenSide &&
                                   SquaresEmpty(new[] {Location.B8, Location.C8, Location.D8}, game) &&
                                   SquaresNotAttacked(color, new[] {Location.C8, Location.D8, Location.E8}, game);
                    }
                }
                    break;
            }

            throw new ArgumentException("Parameters to MayCastle made no sense");
        }

        /// <summary>
        ///     Update the castling status based on the movement of '<paramref name="piece" />' from its original location
        /// </summary>
        /// <param name="piece"></param>
        /// <param name="originalLocation"></param>
        /// <param name="newLocation"></param>
        internal void Update(IPiece piece, Location originalLocation, Location newLocation)
        {
            // All castling options have been exhausted so there's no need to update anything
            if (!(m_whiteMayCastleKingSide || m_whiteMayCastleQueenSide || m_blackMayCastleKingSide || m_blackMayCastleQueenSide))
                return;

            // A white piece has moved, potentially taking the black rooks
            if (piece.Color == PieceColor.White)
            {
                if (newLocation == Location.A8)
                {
                    m_blackMayCastleQueenSide = false;
                }

                if (newLocation == Location.H8)
                {
                    m_blackMayCastleKingSide = false;
                }
            }

            // A black piece has moved, potentially taking the white rooks
            if (piece.Color == PieceColor.Black)
            {
                if (newLocation == Location.A1)
                {
                    m_whiteMayCastleQueenSide = false;
                }

                if (newLocation == Location.H1)
                {
                    m_whiteMayCastleKingSide = false;
                }
            }

            // A rook or a king has moved, update castling options accordingly
            switch (piece.Type)
            {
                case PieceType.WhiteKing:
                    m_whiteMayCastleKingSide = false;
                    m_whiteMayCastleQueenSide = false;
                    break;
                case PieceType.BlackKing:
                    m_blackMayCastleKingSide = false;
                    m_blackMayCastleQueenSide = false;
                    break;
                case PieceType.BlackRook:
                    if (originalLocation == Location.H8)
                        m_blackMayCastleKingSide = false;
                    if (originalLocation == Location.A8)
                        m_blackMayCastleQueenSide = false;
                    break;
                case PieceType.WhiteRook:
                    if (originalLocation == Location.H1)
                        m_whiteMayCastleKingSide = false;
                    if (originalLocation == Location.A1)
                        m_whiteMayCastleQueenSide = false;
                    break;
            }
        }
    }
}