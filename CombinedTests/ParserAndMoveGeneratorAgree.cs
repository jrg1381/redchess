using System;
using System.IO;
using NUnit.Framework;
using RedChess.ChessCommon;
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
            if (text.Contains("[FEN"))
                return;

            var parser = ParserFactory.GetParser();
            
            parser.Parse(text, (s, m, x) =>
            {
                if (x == null)
                {
                    m_board = BoardFactory.CreateInstance();
                    return;
                }

                bool moveOk  = m_board.Move(x.Start, x.End);
                Assert.IsTrue(moveOk, "Expected move to succeed " + m + " " + s + " " + m_board.ToFen());

                if (m_board.IsAwaitingPromotionDecision())
                    m_board.PromotePiece(x.Promotion);

                var lastMove = m_board.LastMove();
                var areEqual = MovesIdenticalWithoutDisambiguator(m, lastMove, x);

                Assert.IsTrue(areEqual, String.Format("Expected parser and move generator to agree: {0} {1}", m, lastMove));
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
        }

        private static bool MovesIdenticalWithoutDisambiguator(string move, string lastMove, ChessMove cm)
        {
            /* My test data disambiguates when it doesn't have to, for example when of the two pieces, one
             * is pinned and cannot move. The move generator follows the spec more closely, and only 
             * disambiguates when it has to */

            return (move == lastMove) || 
                (move[0] + move.Substring(2) == lastMove && 
                (move[1] == cm.Start.ToString().ToLower()[0] || move[1] == cm.Start.ToString().ToLower()[1]));
        }
    }
}
