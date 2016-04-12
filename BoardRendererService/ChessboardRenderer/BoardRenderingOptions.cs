using System.Windows.Media;

namespace RedChess.ChessboardRenderer
{
    public class BoardRenderingOptions
    {
        private readonly Color m_BlackSquares;
        private readonly Color m_WhiteSquares;
        private readonly Color m_SurroundColor;
        private readonly Color m_TextColor;

        // We do this because the brushes are owned by their creating thread, so creating them up front
        // doesn't work properly if a different thread uses the options object (this object)
        internal Brush BlackSquaresBrush => new SolidColorBrush(m_BlackSquares);
        internal Brush WhiteSquaresBrush => new SolidColorBrush(m_WhiteSquares);
        internal Brush SurroundBrush => new SolidColorBrush(m_SurroundColor);
        internal Brush TextBrush => new SolidColorBrush(m_TextColor);

        public BoardRenderingOptions(Color blackSquares, Color whiteSquares, Color surroundColor, Color textColor)
        {
            m_BlackSquares = blackSquares;
            m_WhiteSquares = whiteSquares;
            m_SurroundColor = surroundColor;
            m_TextColor = textColor;
        }
    }
}