using System;
using System.IO;
using System.Security.Cryptography;
using NUnit.Framework;
using RedChess.ChessboardRenderer;

namespace RedChess.RendererTests
{
    [TestFixture]
    public class BasicTests
    {
        private const string c_OutputFilename = "out.png";
        private const string c_FenData = @"rnbqkbnr/pppppppp/8/8/4P3/8/PPPP1PPP/RNBQKBNR b KQkq e3 0";

        private static string FileChecksum(string filename)
        {
            using (var sha256 = new SHA256Managed())
            {
                using (var stream = new FileStream(filename, FileMode.Open))
                {
                    sha256.ComputeHash(stream);
                }

                return Convert.ToBase64String(sha256.Hash);
            }
        }

        [TearDown]
        public void TearDown()
        {
            File.Delete(c_OutputFilename);
        }

        private void WriteOutputFile(BoardRenderingOptions options, int width)
        {
            var boardRenderer = new BoardRenderer(options);
            using (var stream = new FileStream(c_OutputFilename, FileMode.Create))
            {
                boardRenderer.SaveDrawingToStream(c_FenData, stream, width);
            }
        }

        [Test]
        public void RenderPng()
        {
            var options = new BoardRenderingOptions("DimGray", "AntiqueWhite", "SaddleBrown");
            WriteOutputFile(options, 600);

            Assert.AreEqual("nmwdiKD2xwfTJxinMkMjB//VYY7jUwlqHaqKuz4tgAM=", FileChecksum(c_OutputFilename), "Image checksum not as expected");
        }

        [Test]
        public void RenderPngAlternativeColors()
        {
            var options = new BoardRenderingOptions("DimGray", "AntiqueWhite", "Blue");
            WriteOutputFile(options, 800);

            Assert.AreEqual("RgsUvUztmq3y9/gjSIoIwN64Pb4BwZKpcgijwXa0zFc=", FileChecksum(c_OutputFilename), "Image checksum not as expected");
        }
    }
}