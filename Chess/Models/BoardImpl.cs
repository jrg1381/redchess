using RedChess.ChessCommon.Enumerations;
using RedChess.ChessCommon.Interfaces;
using RedChess.EngineFactory;

namespace Chess.Models
{
    /// <summary>
    /// The Model in ASP.NET MVC can't be an interface, but I don't want the website to know about the concrete class that implements IBoard, so we need this local implementation.
    /// </summary>
    public class BoardImpl : IBoard
    {
        private readonly IBoard m_boardImpl;

        public BoardImpl()
        {
            m_boardImpl = BoardFactory.CreateInstance();
        }

        public PieceColor CurrentTurn
        {
            get { return m_boardImpl.CurrentTurn; }
            set { m_boardImpl.CurrentTurn = value; }
        }

        public bool KingInCheck()
        {
            return m_boardImpl.KingInCheck();
        }

        public bool IsCheckmate(bool skipCheckTest)
        {
            return m_boardImpl.IsCheckmate(skipCheckTest);
        }

        public bool IsDraw()
        {
            return m_boardImpl.IsDraw();
        }

        public bool IsStalemate()
        {
            return m_boardImpl.IsStalemate();
        }

        public string ToFen()
        {
            return m_boardImpl.ToFen();
        }

        public void FromFen(string fen)
        {
            m_boardImpl.FromFen(fen);
        }

        public bool Move(Location start, Location end)
        {
            return m_boardImpl.Move(start, end);
        }

        public void PromotePiece(string pieceName)
        {
            m_boardImpl.PromotePiece(pieceName);
        }


        public System.Collections.Generic.IEnumerable<Location> FindPieces(PieceType pieceType)
        {
            return m_boardImpl.FindPieces(pieceType);
        }

        public bool IsAwaitingPromotionDecision()
        {
            return m_boardImpl.IsAwaitingPromotionDecision();
        }
    }
}