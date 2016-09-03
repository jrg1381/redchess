using System;
using System.IO;
using System.Threading;

namespace RedChess.ChessboardRenderer
{
    public class BoardRenderer
    {
        private readonly BoardRenderingOptions m_BoardRenderingOptions;

        public BoardRenderer(BoardRenderingOptions options)
        {
            m_BoardRenderingOptions = options;
        }

        public void SaveDrawingToStream(string fen, Stream outputStream, int widthInPixels)
        {
            Exception exception = null;

            if (widthInPixels < 1 || widthInPixels > 2048)
                throw new ArgumentOutOfRangeException(nameof(widthInPixels));

            var renderingThread = new Thread(() =>
            {
                try
                {
                    var generator = new PngGenerator(m_BoardRenderingOptions);
                    generator.SaveDrawingToStream(fen, outputStream, widthInPixels);
                }
                catch (Exception e)
                {
                    exception = e;
                }
            });

            renderingThread.SetApartmentState(ApartmentState.STA);
            renderingThread.IsBackground = true;
            renderingThread.Name = "Renderer for " + fen;
            renderingThread.Start();
            renderingThread.Join();

            if (exception != null)
            {
                throw new InvalidOperationException("Could not render bitmap", exception);
            }
        }
    }
}
