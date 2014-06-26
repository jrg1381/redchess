using System;
using System.IO;
using Antlr.Runtime;
using NUnit.Framework;
using RedChess.ChessCommon.Interfaces;
using RedChess.ParserFactory;
using RedChess.ParserTests;
using Redchess.Pgn;
using RedChess.PgnProcessor;

namespace Redchess.ParserTests
{
    public class ParserTests
    {
        private const string c_standardTags =
            @"[Event ""helloworld""][Site ""helloworld""][Date ""helloworld""][Round ""helloworld""][White ""helloworld""][Black ""helloworld""][Result ""1-0""]";

        private IParse ParseText(string text)
        {
            var parser = ParserFactory.GetParser();
            parser.Parse(text, null, Assert.Fail, playGame:false);
            return parser;
        }

        [TestCase(@"[Event ""World Championship""]")]
        [TestCase(@"[Event           ""World Championship""]")]
        public void TestIndividualTags(string text)
        {
            var lexer = new PgnLexer(new ANTLRStringStream(text));
            var tokenStream = new CommonTokenStream(lexer);
            var parser = new PgnParser(tokenStream);
            parser.parseTag();
            Assert.AreEqual(0, parser.NumberOfSyntaxErrors, "Syntax error count was non-zero");
            Assert.AreEqual("World Championship", parser.Event);
        }

        [TestCase(
            @"[Event ""World Championship""][Site ""helloworld""][Date ""helloworld""][Round ""helloworld""][White ""helloworld""][Black ""helloworld""][Result ""1-0""] 1. e4 e5 1-0"
            )]
        public void TestCompulsoryTags(string text)
        {
            var parser = ParseText(text);
            Assert.AreEqual("World Championship", parser.Event);
        }

        [TestCase(
            @"[Event ""helloworld""][Site ""helloworld""][Date ""helloworld""][Round ""helloworld""][White ""helloworld""][Black ""helloworld""][Result ""1-0""][MyTag ""something""] 1. e4 e5 1-0"
            )]
        public void TestOptionalTags(string text)
        {
            var parser = ParseText(text);
            Assert.That(parser.Tags["MyTag"] == "something",
                "Expected an optional tag called MyTag with value something");
        }

        [TestCase("1. e4 e5 2. Nf3 Nc6 (3. Ba4) 3. Bb5 a6")]
        [TestCase("1. e4 e5 2. Nf3 Nc6 ((3. Ba4)) 3. Bb5 a6")]
        [TestCase("1. e4 e5 2. Nf3 Nc6 (3. Ba4 (3. Ba4 2. Nf3)) 3. Bb5 a6")]
        [TestCase("1. e4 e5 (1. e4 e5 2. Nf3 Nc6) 2. Nf3 Nc6 (1. e4 e5 2. Nf3 Nc6) 3. Bb5 a6")]
        [TestCase("1. e4 e5 2. Nf3 Nc6 (1. e4 e5 2. Nf3 Nc6 (1. e4 e5 2. Nf3 Nc6) 1. e4 e5 2. Nf3 Nc6) 3. Bb5 a6")]
        [TestCase(
            "1. e4 e5 2. Nf3 Nc6 (1. e4 e5 2. Nf3 Nc6 (1. e4 e5 2. Nf3 Nc6 (1. e4 e5 2. Nf3 Nc6)) 1. e4 e5 2. Nf3 Nc6) 3. Bb5 a6"
            )]
        [TestCase("1. e4 e5 2. Nf3 Nc6 (1. e4 e5 2. Nf3 Nc6 (1. e4 e5 2. Nf3 Nc6) 1. e4 e5 2. Nf3 Nc6) 3. Bb5 a6")]
        [TestCase("1. e4 e5 2. Nf3 Nc6 () 3. Bb5 a6")]
        [TestCase("1. e4 e5 2. Nf3 Nc6 ((1. e4 e5 2. Nf3 Nc6)(1. e4 e5 2. Nf3 Nc6)) 3. Bb5 a6")]
        public void SimpleNestedVariants(string text)
        {
            string z = c_standardTags + " " + text + " 1-0";
            var parser = ParseText(z);
        }

        [TestCase("1. e4 e5 2. Nf3 Nc6 3. Bb5 a6")]
        [TestCase("1. e4 e5 2. Nf3 Nc6 3. Bb5+ a6")]
        [TestCase("1. e4 e5 2. Nf3 Nc6 3. Bb5#")]
        [TestCase("1. e4 e5 2. Nf3 Nc6\r\n3. Bb5#")]
        [TestCase("1. e4 e5 2. Nf3 Nc6+ 3. Bb5 a6#")]
        [TestCase("1. e4 e5 2. Nf3 Nc6+ 3. Bxb5 a6#")]
        [TestCase("1. e4 e5 2. Nf3 Nc6+ 3. Bxb5 a6# { Everything is doomed for white now }")]
        [TestCase("1. e4 e5 2. Nf3 Nc6+ 3. e8=Q a6#")]
        [TestCase("8. c3 O-O 9. h3 O-O-O")]
        [TestCase("9. Qa6xb7# fxg1=Q+")]
        [TestCase(
            @"1. e4 e5 2. Nf3 Nc6 3. Bb5 a6 4. Ba4 Nf6 5. O-O Be7 6. Re1 b5 7. Bb3 d6 8. c3 O-O 9. h3 Nb8 10. d4 Nbd7 11. c4 c6 12. cxb5 axb5 13. Nc3 Bb7 14. Bg5 b4 15. Nb1 h6 16. Bh4 c5 17. dxe5 Nxe4 18. Bxe7 Qxe7 19. exd6 Qf6 20. Nbd2 Nxd6 21. Nc4 Nxc4 22. Bxc4 Nb6 23. Ne5 Rae8 24. Bxf7+ Rxf7 25. Nxf7 Rxe1+ 26. Qxe1 Kxf7 27. Qe3 Qg5 28. Qxg5 hxg5 29. b3 Ke6 30. a3 Kd6 31. axb4 cxb4 32. Ra5 Nd5 33. f3 Bc8 34. Kf2 Bf5 35. Ra7 g6 36. Ra6+ Kc5 37. Ke1 Nf4 38. g3 Nxh3 39. Kd2 Kb5 40. Rd6 Kc5 41. Ra6 Nf2 42. g4 Bd3 43. Re6"
            )]
        [TestCase(@"1. e4 e5 2. Nf3 Nc6 3. Bb5 a6 {This opening is called the Ruy Lopez.}
4. Ba4 Nf6 5. O-O Be7 6. Re1 b5 7. Bb3 d6 8. c3 O-O 9. h3 Nb8  10. d4 Nbd7
11. c4 c6 12. cxb5 axb5 13. Nc3 Bb7 14. Bg5 b4 15. Nb1 h6 16. Bh4 c5 17. dxe5
Nxe4 18. Bxe7 Qxe7 19. exd6 Qf6 20. Nbd2 Nxd6 21. Nc4 Nxc4 22. Bxc4 Nb6
23. Ne5 Rae8 24. Bxf7+ Rxf7 25. Nxf7 Rxe1+ 26. Qxe1 Kxf7 27. Qe3 Qg5 28. Qxg5
hxg5 29. b3 Ke6 30. a3 Kd6 31. axb4 cxb4 32. Ra5 Nd5 33. f3 Bc8 34. Kf2 Bf5
35. Ra7 g6 36. Ra6+ Kc5 37. Ke1 Nf4 38. g3 Nxh3 39. Kd2 Kb5 40. Rd6 Kc5 41. Ra6
Nf2 42. g4 Bd3 43. Re6")]
        public void SomeMoves(string text)
        {
            string z = c_standardTags + " " + text + " 1-0";
            var parser = ParseText(z);
        }

        [TestCase("1. e4 $4 e5 2. Nf3 Nc6 3. Bb5 a6")]
        [TestCase("1. e4 e5 $5 2. Nf3 Nc6 3. Bb5+ a6")]
        [TestCase("1. e4 e5 2. Nf3 $40 Nc6 $100 3. Bb5#")]
        [TestCase("1. e4 e5 2. Nf3 Nc6\r\n3. Bb5# $9")]
        [TestCase("1. e4 $5 e5 $6 2. Nf3 Nc6\r\n3. Bb5# $9")]
        public void NumericAnnotationGlyphs(string text)
        {
            string z = c_standardTags + " " + text + " 1-0";
            var parser = ParseText(z);
        }

        [TestCase(
            "[Event \"helloworld\"]\r\n[Site \"helloworld\"]\r\n[Date \"helloworld\"]\r\n[Round \"helloworld\"]\r\n[White \"helloworld\"]\r\n[Black \"helloworld\"]\r\n[Result \"1-0\"]\r\n1. e4 e5 1-0",
            "1-0")]
        [TestCase(
            @"[Event ""helloworld""][Site ""helloworld""][Date ""helloworld""][Round ""helloworld""][White ""helloworld""][Black ""helloworld""][Result ""0-1""] 1. e4 e5 1-0",
            "0-1")]
        [TestCase(
            @"[Event ""helloworld""][Site ""helloworld""][Date ""helloworld""][Round ""helloworld""][White ""helloworld""][Black ""helloworld""][Result ""1/2-1/2""] 1. e4 e5 0-1",
            "1/2-1/2")]
        [TestCase(
            @"[Event ""helloworld""][Site ""helloworld""][Date ""helloworld""][Round ""helloworld""][White ""helloworld""][Black ""helloworld""][Result ""*""] 1. e4 e5 1-0",
            "*")]
        public void ResultTags(string text, string expectedResult)
        {
            var parser = ParseText(text);
            Assert.AreEqual(expectedResult, parser.Result, "Failed to extract game result");
        }

        [TestCaseSource(typeof (FilenameSource), "PgnFile")]
        public void RealTestData(string filename)
        {
            string inputData = File.ReadAllText(filename);
            ParserFactory.GetParser().Parse(inputData, (s,m) => { }, s =>
            {
                Console.WriteLine(s);
                Assert.Fail("Error in parsing");
            });
        }

        [Test]
        public void EpCaptureCheckTest()
        {
            const string text = @"[Event ""World Championship 07th""]
[Site ""USA""]
[Date ""1907.03.08""]
[Round ""10""]
[White ""Lasker, Emanuel""]
[Black ""Marshall, Frank James""]
[Result ""1/2-1/2""]
[ECO ""C12""]

1. e4 e6 2. d4 d5 3. Nc3 Nf6 4. Bg5 Bb4 5. exd5 Qxd5 6. Bxf6 gxf6 7. Qd2 Bxc3
8. Qxc3 Nc6 9. Nf3 Qe4+ 10. Kd2 Bd7 11. Rd1 O-O-O 12. Kc1 e5 13. Bb5 Nxd4 14.
Nxd4 exd4 15. Rxd4 Qxg2 16. Bxd7+ Rxd7 17. Rxd7 Qxh1+ 18. Rd1 Qxh2 19. Qxf6 Rf8
20. a4 a6 21. Kb1 Qh5 22. Rd3 Re8 23. Qd4 Kb8 24. Qd7 Qh1+ 25. Ka2 Qe4 26. Qxf7
Qxa4+ 27. Ra3 Qc6 28. Qxh7 Qd5+ 29. b3 Re2 30. Ra4 Rxf2 31. Qh8+ Ka7 32. Kb2 c5
33. Rc4 Rf1 34. Ka2 Rf7 35. Qc8 b6 36. Rg4 Qd7 37. Qxd7+ Rxd7 38. Rf4 b5 39.
Rf6 Rd5 40. Kb2 Kb7 41. Rh6 b4 42. Rh7+ Kc6 43. Rh6+ Rd6 44. Rh8 Kb5 45. Rb8+
Rb6 46. Rc8 a5 47. c4+ bxc3+ 48. Kxc3 1/2-1/2";

            string finalFen = "";

            ParserFactory.GetParser().Parse(text, (s,m) => { finalFen = s; }, s =>
            {
                Console.WriteLine(s);
                Assert.Fail("Error in parsing");
            });

            Assert.AreEqual("2R5/8/1r6/pkp5/8/1PK5/8/8 b - -", finalFen, "Expected FEN after playing through game was wrong");
        }

        [Test]
        public void LastMoveIsDetected()
        {
            const string text = @"[Event ""20:42""]
[Site ""Shredder for iPad""]
[Date ""2013.06.29""]
[Round ""?""]
[White ""James""]
[Black ""James""]
[WhiteElo ""1245""]
[BlackElo ""1245""]
[ECO ""C68""]
[Opening ""Ruy Lopez/Exchange Variation""]
[Result ""*""]

1. e4 e5 2. Nf3 Nc6 3. Bb5 a6 4. Bxc6 dxc6 5. d3 Bg4 6. O-O Bxf3 7. Qxf3 f6
8. Nc3 Bb4 9. Bd2 Ne7 10. a3 Bxc3 11. Bxc3 O-O 12. Rad1 b5 13. d4 exd4 14. Bxd4
Qe8 15. Bc5 Rd8 16. Qf4 Rd7 17. Rd3 Ng6 18. Qf5 Rff7 19. Rfd1 Rxd3 20. Rxd3
Qe5 21. Rd8 Nf8 22. Qc8 Qxc5 23. h3 Qxc2 24. b4 Qxe4 25. f3 Qe3 26. Kh2 h6 27. Qxa6
Qf4 28. Kg1 Re7 29. Rd1 Ng6 30. Qa8 Kh7 31. Qxc6 Qe3 32. Kh2 Qf4 33. Kh1 Re1 1-0";

            string finalFen = "";

            ParserFactory.GetParser().Parse(text, (s,m) => { finalFen = s; }, s =>
            {
                Console.WriteLine(s);
                Assert.Fail("Error in parsing");
            });

            Assert.AreEqual("8/2p3pk/2Q2pnp/1p6/1P3q2/P4P1P/6P1/3Rr2K w - -", finalFen, "Expected FEN after playing through game was wrong");

        }
    }
}