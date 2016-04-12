using System.Windows.Media;
using NUnit.Framework;

namespace RedChess.ChessboardRenderer
{
    [TestFixture]
    public class Tests
    {
        [Test]
        public void RenderPng()
        {
            var options = new BoardRenderingOptions(Colors.DimGray, Colors.AntiqueWhite, Colors.SaddleBrown, Colors.Black);
            var boardRenderer = new BoardRenderer(options);
            boardRenderer.SaveDrawingToFile(@"rnbqkbnr/pppppppp/8/8/4P3/8/PPPP1PPP/RNBQKBNR b KQkq e3 0 1", @"c:\users\james.gilmore\out.png", 600);
        }
    }
}
