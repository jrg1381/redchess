using System.Windows.Media;

namespace RedChess.ChessboardRenderer
{
    public class BoardRenderingOptions
    {
        internal Brush BlackSquaresBrush { get; }
        internal Brush WhiteSquaresBrush { get; }
        internal Brush SurroundBrush { get; }

        public BoardRenderingOptions(Color blackSquares, Color whiteSquares, Color surroundColor)
        {
            BlackSquaresBrush = new SolidColorBrush(blackSquares);
            WhiteSquaresBrush = new SolidColorBrush(whiteSquares);
            SurroundBrush = new SolidColorBrush(surroundColor);
        }
    }
}