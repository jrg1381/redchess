using System;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NUnit.Framework;
using RedChess.ChessCommon.Interfaces;
using RedChess.ParserFactory;
using RedChess.EngineFactory;

namespace CombinedTests
{
    [TestFixture]
    public class ParserAndMoveGeneratorAgree
    {
        private IBoard m_board;

        [SetUp]
        public void SetUp()
        {
            m_board = BoardFactory.CreateInstance();
        }

        [TestCaseSource(typeof(GameSource),"Games")]
        public void PlayMoves(string text)
        {
            text = "[Opening \"something\"]\r\n\r\n" + text;
            CheckIdentical(text);
        }

        [TestCaseSource(typeof(FilenameSource), "PgnFile")]
        public void RealTestData(string filename)
        {
            string inputData = File.ReadAllText(filename);
            CheckIdentical(inputData);
        }

        private void CheckIdentical(string text)
        {
            var parser = ParserFactory.GetParser();
            
            parser.Parse(text, (s, m, x) =>
            {
                if (x == null)
                {
                    m_board = BoardFactory.CreateInstance();
                    return;
                }

                bool moveOk  = m_board.Move(x.Item1, x.Item2);
                Assert.IsTrue(moveOk, "Expected move to succeed " + m + " " + s + " " + m_board.ToFen());
                Assert.AreEqual(m, m_board.LastMove(), "Expected parser and move generator to agree");
            }, s =>
            {
                Console.WriteLine(s);
                Assert.Fail("Error in parsing");
            },
                () =>
                {
                    m_board = BoardFactory.CreateInstance();
                },
            true);

            parser.Tags.Clear();
        }
    }
}
