using System.IO;
using System.Windows.Media;
using NUnit.Framework;

namespace RedChess.ChessboardRenderer
{
    [TestFixture]
    public class Tests
    {
        private readonly string m_FenData = @"rnbqkbnr/pppppppp/8/8/4P3/8/PPPP1PPP/RNBQKBNR b KQkq e3 0 1";

        [Test]
        public void RenderPng()
        {
            var options = new BoardRenderingOptions(Colors.DimGray, Colors.AntiqueWhite, Colors.SaddleBrown);
            var boardRenderer = new BoardRenderer(options);
            using (var stream = new FileStream(@"c:\users\james.gilmore\out.png", FileMode.Create))
            {
                boardRenderer.SaveDrawingToStream(m_FenData, stream, 600);
            }
        }

        [Test]
        public void RenderPng2()
        {
            var options = new BoardRenderingOptions(Colors.DimGray, Colors.AntiqueWhite, Colors.Blue);
            var boardRenderer = new BoardRenderer(options);
            using (var stream = new FileStream(@"c:\users\james.gilmore\out2.png", FileMode.Create))
            {
                boardRenderer.SaveDrawingToStream(m_FenData, stream, 800);
            }
        }
    }
}
