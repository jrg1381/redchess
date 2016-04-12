using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;
using NUnit.Framework;

namespace RedChess.ChessboardRenderer
{
    [TestFixture]
    public class Tests
    {
        [Test]
        [STAThread]
        public void RenderPng()
        {
            var options = new BoardRenderingOptions(Colors.DimGray, Colors.AntiqueWhite, Colors.SaddleBrown);
            var boardRenderer = new BoardRenderer(options);
            boardRenderer.SaveDrawingToFile(@"rnbqkbnr/pppppppp/8/8/4P3/8/PPPP1PPP/RNBQKBNR b KQkq e3 0 1", @"c:\users\james.gilmore\out.png", 600);
        }
    }
}
