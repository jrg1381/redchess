namespace RedChess.ChessboardRenderer
{
    internal static class BoardDimensions
    {
        public static readonly int BoardWidth = 512;
        public static readonly int SquareSize = BoardWidth / 8;
        public static readonly int BorderWidth = 10;
        public static readonly int FullWidth = BoardWidth + 2 * BorderWidth;
        public static readonly int PieceMargin = 8;
    }
}