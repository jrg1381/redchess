using System;
using System.IO;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;


namespace RedChess.ChessboardRenderer
{
    internal class PngGenerator
    {
        private readonly BoardRenderingOptions m_Options;
        private DrawingGroup m_DrawingGroup;
        private bool m_RenderCompleted;

        internal PngGenerator(BoardRenderingOptions options)
        {
            m_Options = options;
        }

        private void RenderBoard(string fen)
        {
            if(m_RenderCompleted)
                throw new InvalidOperationException("Cannot render twice");

            // We have to do this here rather than in the constructor because later WPF methods only work when
            // called in an STA thread, and WPF objects (like this one) are only usable in the thread 
            // they're created on.
            m_DrawingGroup = new DrawingGroup();

            using (var context = m_DrawingGroup.Open())
            {
                context.DrawRectangle(
                    m_Options.SurroundBrush,
                    null,
                    new Rect(
                        new Point(0, 0),
                        new Point(
                            BoardDimensions.FullWidth,
                            BoardDimensions.FullWidth)
                        )
                    );

                context.DrawRectangle(
                    m_Options.BlackSquaresBrush,
                    null,
                    new Rect(
                        new Point(
                            BoardDimensions.BorderWidth,
                            BoardDimensions.BorderWidth
                            ),
                        new Vector(
                            BoardDimensions.BoardWidth,
                            BoardDimensions.BoardWidth
                            )
                        )
                    );

                var oneSquare = new Vector(BoardDimensions.SquareSize, BoardDimensions.SquareSize);

                for (var x = 0; x < 8; x++)
                {
                    for (var y = 0; y < 8; y++)
                    {
                        if ((x + y)%2 == 0)
                        {
                            var rectangle = new Rect(
                                new Point(
                                    x*BoardDimensions.SquareSize + BoardDimensions.BorderWidth,
                                    y*BoardDimensions.SquareSize + BoardDimensions.BorderWidth
                                    ),
                                oneSquare
                                );

                            context.DrawRectangle(m_Options.WhiteSquaresBrush, null, rectangle);
                        }
                    }
                }

                var pieceRenderer = new PieceRenderer(context);
                var fenReader = new FenReader(fen);

                fenReader.CallForPieces(pieceRenderer.RenderPiece);
            }

            m_RenderCompleted = true;
        }

        internal void SaveDrawingToStream(string fen, Stream outputStream, int outputWidthInPixels)
        {
            if (!m_RenderCompleted)
            {
                RenderBoard(fen);
            }

            WriteScaledBitmapToStream(outputStream, outputWidthInPixels);
        }

        private void WriteScaledBitmapToStream(Stream outputStream, int outputWidthInPixels)
        {
            var drawingVisual = new DrawingVisual();
            var scale = outputWidthInPixels/m_DrawingGroup.Bounds.Width;

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
            encoder.Save(outputStream);
        }
    }
}