using System;
using System.IO;
using NUnit.Framework;
using RedChess.ChessCommon;
using RedChess.ChessCommon.Interfaces;
using RedChess.EngineFactory;

namespace RedChess.CombinedTests
{
    [TestFixture]
    public class ParserAndMoveGeneratorAgree
    {
        private IBoard m_Board;

        [SetUp]
        public void SetUp()
        {
            m_Board = BoardFactory.CreateInstance();
        }

        [TestCaseSource(typeof(GameSource),nameof(GameSource.Games))]
        public void PlayMoves(string text)
        {
            text = "[Opening \"something\"]\r\n\r\n" + text;
            CheckIdentical(text);
        }

        [TestCaseSource(typeof(FilenameSource), nameof(FilenameSource.PgnFile))]
        public void RealTestData(string filename)
        {
            string inputData = File.ReadAllText(filename);
            CheckIdentical(inputData);
        }

        private void CheckIdentical(string text)
        {
            if (text.Contains("[FEN"))
                return;

            var parser = ParserFactory.ParserFactory.GetParser();
            
            parser.Parse(text, (s, m, x) =>
            {
                if (x == null)
                {
                    m_Board = BoardFactory.CreateInstance();
                    return;
                }

                bool moveOk  = m_Board.Move(x.Start, x.End);
                Assert.IsTrue(moveOk, "Expected move to succeed " + m + " " + s + " " + m_Board.ToFen());

                if (m_Board.IsAwaitingPromotionDecision())
                    m_Board.PromotePiece(x.Promotion);

                var lastMove = m_Board.LastMove();
                var areEqual = MovesIdenticalWithoutDisambiguator(m, lastMove, x);

                Assert.IsTrue(areEqual, $"Expected parser and move generator to agree: {m} {lastMove}");
            }, s =>
            {
                Console.WriteLine(s);
                Assert.Fail("Error in parsing");
            },
                () =>
                {
                    m_Board = BoardFactory.CreateInstance();
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
