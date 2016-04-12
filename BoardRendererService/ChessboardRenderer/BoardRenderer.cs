using System;
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

        public void SaveDrawingToFile(string fen, string filename, int widthInPixels)
        {
            if(widthInPixels < 1 || widthInPixels > 2048)
                throw new ArgumentOutOfRangeException(nameof(widthInPixels));

            var renderingThread = new Thread(() =>
            {
                var generator = new PngGenerator(m_BoardRenderingOptions);
                generator.SaveDrawingToFile(fen, filename, widthInPixels);
            });

            renderingThread.SetApartmentState(ApartmentState.STA);
            renderingThread.IsBackground = true;
            renderingThread.Start();
            renderingThread.Join();
        }
    }
}
