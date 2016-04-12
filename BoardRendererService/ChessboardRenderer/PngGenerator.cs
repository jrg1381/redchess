using System;
using System.Globalization;
using System.IO;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using RedChess.ChessboardRenderer;


namespace RedChess.ChessboardRenderer
{
    internal class PngGenerator
    {
        private readonly BoardRenderingOptions m_Options;
        private readonly DrawingGroup m_DrawingGroup;
        private bool m_RenderCompleted;

        internal PngGenerator(BoardRenderingOptions options)
        {
            m_Options = options;
            m_DrawingGroup = new DrawingGroup();
        }

        private void RenderBoard(string fen)
        {
            if(m_RenderCompleted)
                throw new InvalidOperationException("Cannot render twice");

            using (var context = m_DrawingGroup.Open())
            {
                context.DrawRectangle(m_Options.SurroundBrush, null,
                    new Rect(new Point(0, 0), new Point(BoardDimensions.FullWidth, BoardDimensions.FullWidth)));
                context.DrawRectangle(m_Options.BlackSquaresBrush, null,
                    new Rect(new Point(BoardDimensions.BorderWidth, BoardDimensions.BorderWidth), new Vector(BoardDimensions.BoardWidth, BoardDimensions.BoardWidth)));

                for (var x = 0; x < 8; x++)
                {
                    for (var y = 0; y < 8; y++)
                    {
                        if ((x + y)%2 == 0)
                        {
                            var rectangle = new Rect(
                                new Point(x* BoardDimensions.SquareSize + BoardDimensions.BorderWidth, y* BoardDimensions.SquareSize + BoardDimensions.BorderWidth),
                                new Vector(BoardDimensions.SquareSize, BoardDimensions.SquareSize));

                            context.DrawRectangle(m_Options.WhiteSquaresBrush, null, rectangle);
                        }
                    }
                }

                for (var i = 0; i < 8; i++)
                {
                    var offset = BoardDimensions.BorderWidth + i* BoardDimensions.SquareSize + BoardDimensions.SquareSize/ 2.0;

                    context.DrawText(
                        new FormattedText("ABCDEFGH"[i].ToString(),
                            CultureInfo.CurrentCulture,
                            FlowDirection.LeftToRight,
                            new Typeface("Arial"),
                            15,
                            Brushes.Black), new Point(offset, BoardDimensions.FullWidth - BoardDimensions.BorderWidth));

                    context.DrawText(
                        new FormattedText("87654321"[i].ToString(),
                            CultureInfo.CurrentCulture,
                            FlowDirection.LeftToRight,
                            new Typeface("Arial"),
                            15,
                            Brushes.Black), new Point(BoardDimensions.BorderWidth/ 2.0, offset));
                }

                var pieceRenderer = new PieceRenderer(context);
                var fenReader = new FenReader(fen);

                fenReader.CallForPieces(pieceRenderer.RenderPiece);
            }

            m_RenderCompleted = true;
        }

        internal void SaveDrawingToFile(string fen, string fileName, int outputWidthInPixels)
        {
            if (!m_RenderCompleted)
            {
                RenderBoard(fen);
            }

            var drawingVisual = new DrawingVisual();
            var scale = outputWidthInPixels / m_DrawingGroup.Bounds.Width;

            using (var drawingContext = drawingVisual.RenderOpen())
            {
                drawingContext.PushTransform(new ScaleTransform(scale, scale));
                foreach (var drawing in m_DrawingGroup.Children)
                {
                    drawingContext.DrawDrawing(drawing);
                }
            }

            var bitmap = new RenderTargetBitmap(outputWidthInPixels, outputWidthInPixels, 96, 96, PixelFormats.Pbgra32);
            bitmap.Render(drawingVisual);

            var encoder = new PngBitmapEncoder();
            encoder.Frames.Add(BitmapFrame.Create(bitmap));

            using (var stream = new FileStream(fileName, FileMode.Create))
            {
                encoder.Save(stream);
            }
        }
    }
}