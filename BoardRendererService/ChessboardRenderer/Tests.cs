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
            var renderer = new PngGenerator(options);
            renderer.RenderBoard();
            renderer.SaveDrawingToFile(@"c:\users\james.gilmore\out.png", 600);
        }
    }
}
