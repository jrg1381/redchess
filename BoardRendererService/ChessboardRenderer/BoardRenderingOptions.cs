using System.Windows.Media;

namespace RedChess.ChessboardRenderer
{
    internal class BoardRenderingOptions
    {
        public Brush BlackSquaresBrush { get; }
        public Brush WhiteSquaresBrush { get; }
        public Brush SurroundBrush { get; }

        public BoardRenderingOptions(Color blackSquares, Color whiteSquares, Color surroundColor)
        {
            BlackSquaresBrush = new SolidColorBrush(blackSquares);
            WhiteSquaresBrush = new SolidColorBrush(whiteSquares);
            SurroundBrush = new SolidColorBrush(surroundColor);
        }
    }
}