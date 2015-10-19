using System;
using System.IO;
using Antlr4.Runtime;
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
            parser.Parse(text, null, Assert.Fail, null, playGame:false);
            return parser;
        }

        [TestCase(@"[Event ""World Championship""]")]
        [TestCase(@"[Event           ""World Championship""]")]
        public void TestIndividualTags(string text)
        {
            var lexer = new PgnLexer(new AntlrInputStream(text));
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

        [TestCase("1. e4! e5 2. Nf3 Nc6 3. Bb5 a6")]
        [TestCase("1. e4 e5!! 2. Nf3 Nc6 3. Bb5+ a6")]
        [TestCase("1. e4 e5 2. Nf3? Nc6 3. Bb5#")]
        [TestCase("1. e4 e5 2. Nf3 Nc6??\r\n3. Bb5#")]
        [TestCase("1. e4!? e5?! 2. Nf3 Nc6\r\n3. Bb5#")]
        public void MoveAnalysisMarkers(string text)
        {
            string z = c_standardTags + " " + text + " 1-0";
            var parser = ParseText(z);
        }

        [Test]
        public void InvalidVariant()
        {
            var text = @"1.d4 {+0.18/5 0s} Nf6 {+0.06/5 0s} 2.e3 {+0.01/11 0s} d5 {+0.05/11 2s} 
3.Nf3 {+0.05/10 0s} e6 {+0.14/10 2s} 4.c3 {-0.08/11 1s} Bd6 {-0.07/11 1s} 
5.Ne5 {-0.19/10 0s} Bxe5 {+0.12/12 1s} 6.dxe5 {+0.12/12 1s} Nfd7 {+0.11/11 0s} 
7.Qh5 {-0.11/12 0s} O-O {-0.18/11 0s} 8.Bd3 {-0.67/12 0s} (8.f4 {-0.08/11 3s} 
b6 9.b3 Nc5 10.Ba3 Nbd7 11.Bb5 a6 12.Bc6 Nd3+ 13.Kf1 g6) 8. ... g6";
            string z = c_standardTags + " " + text + " 1-0";
            var parser = ParseText(z);
        }

        [Test]
        public void LichessPgnOutput()
        {
            var text = @"1. c4 c5 2. Nf3 Nc6 3. e3 e6 4. Nc3 Nf6 5. d4 a6?! { (0.05 ? 0.83) Inaccuracy. The best move was d5. } (5... d5 6. cxd5) 6. Bd3?! { (0.83 ? 0.10) Inaccuracy. The best move was d5. } (6. d5 Ne7 7. e4 Ng6 8. h4 d6 9. h5 Ne7 10. Be2 exd5 11. exd5 Bg4 12. h6 gxh6 13. Qb3 Qc7 14. Bxh6 Bxh6 15. Rxh6) 6... cxd4 7. exd4 b5 8. cxb5 axb5 9. O-O?! { (0.75 ? 0.11) Inaccuracy. The best move was Bxb5. } (9. Bxb5 Ba6 10. Bxa6 Rxa6 11. O-O Bb4 12. d5 Ne7 13. dxe6 fxe6 14. Qe2 Ra8 15. Ne5 O-O 16. Bg5 h6 17. Bxf6 Rxf6 18. Qg4 Bd6) 9... b4 10. Ne4 Ba6?! { (-0.24 ? 0.50) Inaccuracy. The best move was Nxe4. } (10... Nxe4 11. Bxe4 d5 12. Bc2 Bd6 13. Be3 Bb7 14. Re1 Qc7 15. g3 h6 16. Qd2 Ba6 17. Bf4 Bxf4 18. gxf4 O-O 19. f5 b3 20. axb3) 11. Bg5 Be7 12. Nxf6+?! { (0.71 ? 0.16) Inaccuracy. The best move was Bxf6. } (12. Bxf6 Bxf6 13. d5 Ne7 14. dxe6 fxe6 15. Nd6+ Kf8 16. Re1 Bxb2 17. Be4 Rb8 18. Rb1 Bc3 19. Re3 Bf6 20. a3 Qc7 21. Rc1 Qa5) 12... Bxf6 13. Bxf6 Qxf6 14. Bxa6 Rxa6 15. Qd3 Rb6 16. b3?! { (0.40 ? -0.18) Inaccuracy. The best move was d5. } (16. d5 Ne5 17. Nxe5 Qxe5 18. dxe6 dxe6 19. a4 Rd6 20. Qc4 O-O 21. Qxb4 Rd4 22. Qe7) 16... d5 17. Rac1 O-O 18. Rc5 Ra8 19. Qd2 Rba6?! { (0.00 ? 0.65) Inaccuracy. The best move was g5. } (19... g5 20. Qc1 Raa6 21. Qe3 Qf4 22. Rc2 Qxe3 23. fxe3 g4 24. Ng5 Rb7 25. Rcf2 Raa7 26. Rc1 Ra6 27. Rcf1) 20. Rfc1 Rxa2? { (0.59 ? 2.83) Mistake. The best move was Nd8. } (20... Nd8 21. R1c2 Qh6 22. Qe2 g6 23. Rc8) 21. Rxc6 Ra1?! { (2.67 ? 3.56) Inaccuracy. The best move was Rxd2. } (21... Rxd2 22. Rc8+ Qd8 23. Rxd8+ Rxd8 24. Nxd2 Ra8 25. f4 h6 26. Kf2 g5 27. fxg5 hxg5 28. Rc7 Ra1 29. Rb7 Rd1 30. Nf3 g4 31. Ng5) 22. Rc8+ Rxc8 23. Rxa1 Qe7 24. Ne5 f6 25. Nd3 Rc3 26. Ra4 Rxb3?? { (3.47 ? 7.77) Blunder. The best move was Qf8. } (26... Qf8 27. Qd1 h6 28. g3 Qc8 29. Kg2 Qb7) 27. h3?? { (7.77 ? 2.72) Blunder. The best move was Ra8+. } (27. Ra8+ Qf8 28. Rxf8+ Kxf8 29. Qd1 Rc3 30. Nxb4 Rc4 31. Na6 Kf7 32. Nc5 h6 33. Qa1 g5 34. Qb1 f5 35. Qe1 e5 36. Qb1 e4) 27... Rb1+ 28. Kh2 b3?? { (3.17 ? 7.53) Blunder. The best move was Qe8. } (28... Qe8 29. Rxb4 Rxb4 30. Qxb4 Qd8 31. Qb7 Kf8 32. Kg3 Qe7 33. Qa6 Kf7 34. Nc5 Qe8 35. Kf3 g5 36. Kg4 h6 37. Kf3 h5 38. g3) 29. Qb4?? { (7.53 ? 2.00) Blunder. The best move was Ra8+. } (29. Ra8+ Kf7 30. Qf4 g5 31. Qb8 Kg6 32. Ra7 Qxa7 33. Qxa7 Rd1 34. Qa6 Rd2 35. Qxe6 Rxd3) 29... Qc7+?! { (2.00 ? 2.56) Inaccuracy. The best move was Qxb4. } (29... Qxb4 30. Rxb4 g5 31. Kg3 Kf7 32. Rb6 h5 33. Kf3 f5 34. Ne5+ Ke7 35. Rb7+ Kd8 36. Nd3 Kc8 37. Nc5 b2) 30. g3 h5?? { (2.63 ? 5.93) Blunder. The best move was Kf7. } (30... Kf7 31. Nc5 b2 32. Ra2 h6 33. Rxb2 Rxb2 34. Qxb2 e5 35. Kg2 Qc6 36. f3 g6 37. f4 Kg7 38. Kf3 h5 39. Qb4 exf4 40. Kxf4) 31. Nc5?? { (5.93 ? 2.13) Blunder. The best move was Ra8+. } (31. Ra8+ Kh7 32. Qf8 b2 33. Rb8 Rd1 34. Qh8+ Kg6 35. Nf4+ Qxf4 36. gxf4 b1=Q 37. Qe8+ Kh6 38. Rxb1 Rxb1 39. Qxe6 Rb5 40. f5 Rb2) 31... h4 32. Qc3?! { (2.17 ? 1.47) Inaccuracy. The best move was Ra8+. } (32. Ra8+ Kh7 33. Qc3 hxg3+ 34. fxg3 f5 35. Ra3 e5 36. Rxb3 exd4 37. Qxd4 Rc1 38. Rc3 Rxc3 39. Qxc3 Qe7 40. Kg1 Qe2 41. Qd3 Qxd3) 32... Kf7?! { (1.47 ? 2.21) Inaccuracy. The best move was hxg3+. } (32... hxg3+ 33. fxg3 Qb8 34. Nd3 Qe8 35. Rb4 Qg6 36. Rb8+ Kh7 37. Rxb3 Rxb3 38. Qxb3) 33. Rb4 hxg3+ 34. fxg3 Qa5?? { (2.29 ? Mate in 11) Checkmate is now unavoidable. The best move was Qc8. } (34... Qc8 35. Rb7+ Kg8 36. Rxb3 Rxb3 37. Qxb3 Qc6 38. Qb8+ Kf7 39. Qa7+ Kg6 40. Qa2 e5 41. Qf2 Qd6 42. h4 Kf7) 35. Rb7+ Kg6 36. Qxa5 Rb2+ 37. Kg1 Rb1+ 38. Kf2 b2 39. Qd2 Rf1+";
            string z = c_standardTags + " " + text + " 1-0";
            var parser = ParseText(z);
        }

        [Test]
        public void CommentInsideVariant()
        {
            var text = @"1. c4 c5 2. Nf3 Nc6 3. e3 e6 4. Nc3 Nf6 5. d4 a6?! ({some comment} 5... d5 6. cxd5) 6. Bd3?!";
            string z = c_standardTags + " " + text + " 1-0";
            var parser = ParseText(z);
        }

        [Test]
        public void CommentInsideVariantNoSpace()
        {
            var text = @"1. c4 c5 2. Nf3 Nc6 3. e3 e6 4. Nc3 Nf6 5. d4 a6?! ({some comment}5... d5 6. cxd5) 6. Bd3?!";
            string z = c_standardTags + " " + text + " 1-0";
            var parser = ParseText(z);
        }

        [Test]
        public void ParserErrorHandlerIsCalled()
        {
            string errorMsg = "";
            // Expect error handler to detect extra c5 move
            var text = c_standardTags + @" 1. c4 c5 c5 1-0";
            var parser = ParserFactory.GetParser();
            parser.Parse(text, null, s => errorMsg = s, null, playGame: false);
            Assert.AreEqual("Error in parser at line 1:142[c5]", errorMsg, "Error message not as expected");
        }

        [Test]
        public void LexerErrorHandlerIsCalled()
        {
            string errorMsg = "";
            // Expect error handler to detect extra c5 move
            var text = c_standardTags + @" 1. c4 c5 ^^^^";
            var parser = ParserFactory.GetParser();
            parser.Parse(text, null, s => errorMsg = s, null, playGame: false);
            Assert.AreEqual("token recognition error at: '^'", errorMsg, "Error message not as expected");
        }

        [Test]
        public void LichessPgnOutput2()
        {
            var text = @"1. c4 c5 2. Nf3 Nc6 3. e3 e6 4. Nc3 Nf6 5. d4 a6?! (5... d5 6. cxd5) 6. Bd3?!";
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
            ParserFactory.GetParser().Parse(inputData, (s,m,x) => { }, s =>
            {
                Console.WriteLine(s);
                Assert.Fail("Error in parsing");
            }, null);
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

            ParserFactory.GetParser().Parse(text, (s,m,x) => { finalFen = s; }, s =>
            {
                Console.WriteLine(s);
                Assert.Fail("Error in parsing");
            }, null);

            Assert.AreEqual("2R5/8/1r6/pkp5/8/1PK5/8/8 b - - 0", finalFen, "Expected FEN after playing through game was wrong");
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

            ParserFactory.GetParser().Parse(text, (s,m,x) => { finalFen = s; }, s =>
            {
                Console.WriteLine(s);
                Assert.Fail("Error in parsing");
            }, null);

            Assert.AreEqual("8/2p3pk/2Q2pnp/1p6/1P3q2/P4P1P/6P1/3Rr2K w - - 5", finalFen, "Expected FEN after playing through game was wrong");

        }
    }
}