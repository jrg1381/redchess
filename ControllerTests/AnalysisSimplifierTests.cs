using NUnit.Framework;
using RedChess.ChessCommon;
using RedChess.ChessCommon.Enumerations;
using RedChess.WebEngine.Models;
using RedChess.WebEngine.Repositories;
using RedChess.WebEngine.Repositories.Interfaces;
using Rhino.Mocks;

namespace RedChess.ControllerTests
{
    [TestFixture]
    public class AnalysisSimplifierTests
    {
        IHistoryRepository GetFakeHistoryRepo(HistoryEntry historyEntry)
        {
            var mock = MockRepository.GenerateMock<IHistoryRepository>();

            mock.Expect(m => m.FindByGameIdAndMoveNumber(historyEntry.GameId, historyEntry.MoveNumber))
                .Return(historyEntry);

            return mock;
        }

        [Test]
        public void CentipawnModified()
        {
            var historyEntry = new HistoryEntry
            {
                GameId = 10,
                MoveNumber = 1,
                Fen = "rnbqkbnr/pppppppp/8/8/8/8/PPPPPPPP/RNBQKBNR w KQkq - 0",
                Move = "Nc3"
            };

            var mock = GetFakeHistoryRepo(historyEntry);
            var processor = new AnalysisSimplifier(mock);
            var boardAnalysis = new BoardAnalysis
            {
                BoardEvaluation = 44,
                Analysis = "info depth 6 seldepth 6 multipv 1 score cp 21 nodes 4562 nps 350923 tbhits 0 time 13 pv g1f3 g8f6 d2d4 d7d5 b1c3 e7e6 info",
                BoardEvaluationType = EvaluationType.Centipawn
            };

            var expectedAnalysis = new BoardAnalysis
            {
                BoardEvaluation = 44,
                Analysis = "Nf3 Nf6 d4 d5 Nc3 e6",
                BoardEvaluationType = EvaluationType.Centipawn
            };

            var newAnalysis = processor.ProcessBoardAnalysis(10, 1, boardAnalysis);
            Assert.AreEqual(expectedAnalysis, newAnalysis, "Expected analysis text to be unchanged for non-mate analysis");
            mock.VerifyAllExpectations();
        }

        [Test]
        public void MateByPromotion()
        {
            var historyEntry = new HistoryEntry
            {
                GameId = 10,
                MoveNumber = 23,
                Fen = "k7/7R/6P1/8/8/8/8/7K w - - 0",
                Move = "Ka8"
            };

            var mock = GetFakeHistoryRepo(historyEntry);
            var processor = new AnalysisSimplifier(mock);
            var boardAnalysis = new BoardAnalysis
            {
                BoardEvaluation = 1,
                Analysis = "score mate 1 nodes 4884896 nps 1051419 tbhits 0 time 4646 pv g6g7 a8b8 g7g8q info depth 34",
                BoardEvaluationType = EvaluationType.MateInN
            };

            var expectedAnalysis = new BoardAnalysis
            {
                BoardEvaluation = 1,
                Analysis = "g7 Kb8 g8=Q#",
                BoardEvaluationType = EvaluationType.MateInN
            };

            var newAnalysis = processor.ProcessBoardAnalysis(10, 23, boardAnalysis);
            Assert.AreEqual(expectedAnalysis, newAnalysis, "Expected analysis text to have move substituted in");
            mock.VerifyAllExpectations();
        }

        [Test]
        public void MateInNModified()
        {
            var historyEntry = new HistoryEntry
            {
                GameId = 10,
                MoveNumber = 1,
                Fen = "rnbqkbnr/pppppppp/8/8/8/8/PPPPPPPP/RNBQKBNR w KQkq - 0",
                Move = ""
            };

            var mock = GetFakeHistoryRepo(historyEntry);
            var processor = new AnalysisSimplifier(mock);
            var boardAnalysis = new BoardAnalysis
            {
                BoardEvaluation = -11,
                Analysis = "score mate -11 nodes 4884896 nps 1051419 tbhits 0 time 4646 pv b1c3 b8c6 info depth 34",
                BoardEvaluationType = EvaluationType.MateInN
            };

            var expectedAnalysis = new BoardAnalysis
            {
                BoardEvaluation = -11,
                Analysis = "Nc3 Nc6",
                BoardEvaluationType = EvaluationType.MateInN
            };

            var newAnalysis = processor.ProcessBoardAnalysis(10, 1, boardAnalysis);
            Assert.AreEqual(expectedAnalysis, newAnalysis, "Expected analysis text to have move substituted in");
            mock.VerifyAllExpectations();
        }

        [Test]
        public void LongAnalysis()
        {
            #region data
            string data = @"info depth 1 seldepth 2 multipv 1 score mate 1 nodes 37 nps 37000 tbhits 0 time 1 pv d8h4 
 info depth 2 seldepth 2 multipv 1 score mate 1 nodes 69 nps 69000 tbhits 0 time 1 pv d8h4
 info depth 3 seldepth 2 multipv 1 score mate 1 nodes 101 nps 101000 tbhits 0 time 1 pv d8h4
 info depth 4 seldepth 2 multipv 1 score mate 1 nodes 133 nps 133000 tbhits 0 time 1 pv d8h4
 info depth 5 seldepth 2 multipv 1 score mate 1 nodes 165 nps 165000 tbhits 0 time 1 pv d8h4
 info depth 6 seldepth 2 multipv 1 score mate 1 nodes 197 nps 197000 tbhits 0 time 1 pv d8h4
 info depth 7 seldepth 2 multipv 1 score mate 1 nodes 229 nps 229000 tbhits 0 time 1 pv d8h4
 info depth 8 seldepth 2 multipv 1 score mate 1 nodes 261 nps 261000 tbhits 0 time 1 pv d8h4
 info depth 9 seldepth 2 multipv 1 score mate 1 nodes 293 nps 293000 tbhits 0 time 1 pv d8h4
 info depth 10 seldepth 2 multipv 1 score mate 1 nodes 325 nps 325000 tbhits 0 time 1 pv d8h4
 info depth 11 seldepth 2 multipv 1 score mate 1 nodes 357 nps 357000 tbhits 0 time 1 pv d8h4
 info depth 12 seldepth 2 multipv 1 score mate 1 nodes 389 nps 389000 tbhits 0 time 1 pv d8h4
 info depth 13 seldepth 2 multipv 1 score mate 1 nodes 421 nps 421000 tbhits 0 time 1 pv d8h4
 info depth 14 seldepth 2 multipv 1 score mate 1 nodes 453 nps 453000 tbhits 0 time 1 pv d8h4
 info depth 15 seldepth 2 multipv 1 score mate 1 nodes 485 nps 485000 tbhits 0 time 1 pv d8h4
 info depth 16 seldepth 2 multipv 1 score mate 1 nodes 517 nps 517000 tbhits 0 time 1 pv d8h4
 info depth 17 seldepth 2 multipv 1 score mate 1 nodes 549 nps 549000 tbhits 0 time 1 pv d8h4
 info depth 18 seldepth 2 multipv 1 score mate 1 nodes 581 nps 581000 tbhits 0 time 1 pv d8h4
 info depth 19 seldepth 2 multipv 1 score mate 1 nodes 613 nps 613000 tbhits 0 time 1 pv d8h4
 info depth 20 seldepth 2 multipv 1 score mate 1 nodes 645 nps 645000 tbhits 0 time 1 pv d8h4
 info depth 21 seldepth 2 multipv 1 score mate 1 nodes 677 nps 52076 tbhits 0 time 13 pv d8h4
 info depth 22 seldepth 2 multipv 1 score mate 1 nodes 709 nps 54538 tbhits 0 time 13 pv d8h4
 info depth 23 seldepth 2 multipv 1 score mate 1 nodes 741 nps 57000 tbhits 0 time 13 pv d8h4
 info depth 24 seldepth 2 multipv 1 score mate 1 nodes 773 nps 59461 tbhits 0 time 13 pv d8h4
 info depth 25 seldepth 2 multipv 1 score mate 1 nodes 805 nps 61923 tbhits 0 time 13 pv d8h4
 info depth 26 seldepth 2 multipv 1 score mate 1 nodes 837 nps 64384 tbhits 0 time 13 pv d8h4
 info depth 27 seldepth 2 multipv 1 score mate 1 nodes 869 nps 66846 tbhits 0 time 13 pv d8h4
 info depth 28 seldepth 2 multipv 1 score mate 1 nodes 901 nps 69307 tbhits 0 time 13 pv d8h4
 info depth 29 seldepth 2 multipv 1 score mate 1 nodes 933 nps 71769 tbhits 0 time 13 pv d8h4
 info depth 30 seldepth 2 multipv 1 score mate 1 nodes 965 nps 32166 tbhits 0 time 30 pv d8h4
 info depth 31 seldepth 2 multipv 1 score mate 1 nodes 997 nps 33233 tbhits 0 time 30 pv d8h4
 info depth 32 seldepth 2 multipv 1 score mate 1 nodes 1029 nps 34300 tbhits 0 time 30 pv d8h4
 info depth 33 seldepth 2 multipv 1 score mate 1 nodes 1061 nps 35366 tbhits 0 time 30 pv d8h4
 info depth 34 seldepth 2 multipv 1 score mate 1 nodes 1093 nps 36433 tbhits 0 time 30 pv d8h4
 info depth 35 seldepth 2 multipv 1 score mate 1 nodes 1125 nps 37500 tbhits 0 time 30 pv d8h4
 info depth 36 seldepth 2 multipv 1 score mate 1 nodes 1157 nps 38566 tbhits 0 time 30 pv d8h4
 info depth 37 seldepth 2 multipv 1 score mate 1 nodes 1189 nps 39633 tbhits 0 time 30 pv d8h4
 info depth 38 seldepth 2 multipv 1 score mate 1 nodes 1221 nps 40700 tbhits 0 time 30 pv d8h4
 info depth 39 seldepth 2 multipv 1 score mate 1 nodes 1253 nps 41766 tbhits 0 time 30 pv d8h4
 info depth 40 seldepth 2 multipv 1 score mate 1 nodes 1285 nps 26224 tbhits 0 time 49 pv d8h4
 info depth 41 seldepth 2 multipv 1 score mate 1 nodes 1317 nps 26877 tbhits 0 time 49 pv d8h4
 info depth 42 seldepth 2 multipv 1 score mate 1 nodes 1349 nps 27530 tbhits 0 time 49 pv d8h4
 info depth 43 seldepth 2 multipv 1 score mate 1 nodes 1381 nps 28183 tbhits 0 time 49 pv d8h4
 info depth 44 seldepth 2 multipv 1 score mate 1 nodes 1413 nps 23550 tbhits 0 time 60 pv d8h4
 info depth 45 seldepth 2 multipv 1 score mate 1 nodes 1445 nps 24083 tbhits 0 time 60 pv d8h4
 info depth 46 seldepth 2 multipv 1 score mate 1 nodes 1477 nps 24616 tbhits 0 time 60 pv d8h4
 info depth 47 seldepth 2 multipv 1 score mate 1 nodes 1509 nps 25150 tbhits 0 time 60 pv d8h4
 info depth 48 seldepth 2 multipv 1 score mate 1 nodes 1541 nps 25683 tbhits 0 time 60 pv d8h4
 info depth 49 seldepth 2 multipv 1 score mate 1 nodes 1573 nps 26216 tbhits 0 time 60 pv d8h4
 info depth 50 seldepth 2 multipv 1 score mate 1 nodes 1605 nps 26750 tbhits 0 time 60 pv d8h4
 info depth 51 seldepth 2 multipv 1 score mate 1 nodes 1637 nps 20721 tbhits 0 time 79 pv d8h4
 info depth 52 seldepth 2 multipv 1 score mate 1 nodes 1669 nps 21126 tbhits 0 time 79 pv d8h4
 info depth 53 seldepth 2 multipv 1 score mate 1 nodes 1701 nps 21531 tbhits 0 time 79 pv d8h4
 info depth 54 seldepth 2 multipv 1 score mate 1 nodes 1733 nps 21936 tbhits 0 time 79 pv d8h4
 info depth 55 seldepth 2 multipv 1 score mate 1 nodes 1765 nps 22341 tbhits 0 time 79 pv d8h4
 info depth 56 seldepth 2 multipv 1 score mate 1 nodes 1797 nps 19747 tbhits 0 time 91 pv d8h4
 info depth 57 seldepth 2 multipv 1 score mate 1 nodes 1829 nps 20098 tbhits 0 time 91 pv d8h4
 info depth 58 seldepth 2 multipv 1 score mate 1 nodes 1861 nps 20450 tbhits 0 time 91 pv d8h4
 info depth 59 seldepth 2 multipv 1 score mate 1 nodes 1893 nps 17691 tbhits 0 time 107 pv d8h4
 info depth 60 seldepth 2 multipv 1 score mate 1 nodes 1925 nps 17990 tbhits 0 time 107 pv d8h4
 info depth 61 seldepth 2 multipv 1 score mate 1 nodes 1957 nps 18289 tbhits 0 time 107 pv d8h4
 info depth 62 seldepth 2 multipv 1 score mate 1 nodes 1989 nps 18588 tbhits 0 time 107 pv d8h4
 info depth 63 seldepth 2 multipv 1 score mate 1 nodes 2021 nps 18887 tbhits 0 time 107 pv d8h4
 info depth 64 seldepth 2 multipv 1 score mate 1 nodes 2053 nps 16165 tbhits 0 time 127 pv d8h4
 info depth 65 seldepth 2 multipv 1 score mate 1 nodes 2085 nps 16417 tbhits 0 time 127 pv d8h4
 info depth 66 seldepth 2 multipv 1 score mate 1 nodes 2117 nps 16669 tbhits 0 time 127 pv d8h4
 info depth 67 seldepth 2 multipv 1 score mate 1 nodes 2149 nps 16921 tbhits 0 time 127 pv d8h4
 info depth 68 seldepth 2 multipv 1 score mate 1 nodes 2181 nps 15804 tbhits 0 time 138 pv d8h4
 info depth 69 seldepth 2 multipv 1 score mate 1 nodes 2213 nps 16036 tbhits 0 time 138 pv d8h4
 info depth 70 seldepth 2 multipv 1 score mate 1 nodes 2245 nps 16268 tbhits 0 time 138 pv d8h4
 info depth 71 seldepth 2 multipv 1 score mate 1 nodes 2277 nps 16500 tbhits 0 time 138 pv d8h4
 info depth 72 seldepth 2 multipv 1 score mate 1 nodes 2309 nps 14801 tbhits 0 time 156 pv d8h4
 info depth 73 seldepth 2 multipv 1 score mate 1 nodes 2341 nps 15006 tbhits 0 time 156 pv d8h4
 info depth 74 seldepth 2 multipv 1 score mate 1 nodes 2373 nps 15211 tbhits 0 time 156 pv d8h4
 info depth 75 seldepth 2 multipv 1 score mate 1 nodes 2405 nps 15416 tbhits 0 time 156 pv d8h4
 info depth 76 seldepth 2 multipv 1 score mate 1 nodes 2437 nps 14420 tbhits 0 time 169 pv d8h4
 info depth 77 seldepth 2 multipv 1 score mate 1 nodes 2469 nps 14609 tbhits 0 time 169 pv d8h4
 info depth 78 seldepth 2 multipv 1 score mate 1 nodes 2501 nps 13518 tbhits 0 time 185 pv d8h4
 info depth 79 seldepth 2 multipv 1 score mate 1 nodes 2533 nps 13691 tbhits 0 time 185 pv d8h4
 info depth 80 seldepth 2 multipv 1 score mate 1 nodes 2565 nps 13864 tbhits 0 time 185 pv d8h4
 info depth 81 seldepth 2 multipv 1 score mate 1 nodes 2597 nps 14037 tbhits 0 time 185 pv d8h4
 info depth 82 seldepth 2 multipv 1 score mate 1 nodes 2629 nps 12887 tbhits 0 time 204 pv d8h4
 info depth 83 seldepth 2 multipv 1 score mate 1 nodes 2661 nps 13044 tbhits 0 time 204 pv d8h4
 info depth 84 seldepth 2 multipv 1 score mate 1 nodes 2693 nps 13200 tbhits 0 time 204 pv d8h4
 info depth 85 seldepth 2 multipv 1 score mate 1 nodes 2725 nps 12615 tbhits 0 time 216 pv d8h4
 info depth 86 seldepth 2 multipv 1 score mate 1 nodes 2757 nps 12763 tbhits 0 time 216 pv d8h4
 info depth 87 seldepth 2 multipv 1 score mate 1 nodes 2789 nps 12912 tbhits 0 time 216 pv d8h4
 info depth 88 seldepth 2 multipv 1 score mate 1 nodes 2821 nps 12004 tbhits 0 time 235 pv d8h4
 info depth 89 seldepth 2 multipv 1 score mate 1 nodes 2853 nps 12140 tbhits 0 time 235 pv d8h4
 info depth 90 seldepth 2 multipv 1 score mate 1 nodes 2885 nps 12276 tbhits 0 time 235 pv d8h4
 info depth 91 seldepth 2 multipv 1 score mate 1 nodes 2917 nps 11809 tbhits 0 time 247 pv d8h4
 info depth 92 seldepth 2 multipv 1 score mate 1 nodes 2949 nps 11939 tbhits 0 time 247 pv d8h4
 info depth 93 seldepth 2 multipv 1 score mate 1 nodes 2981 nps 12068 tbhits 0 time 247 pv d8h4
 info depth 94 seldepth 2 multipv 1 score mate 1 nodes 3013 nps 11412 tbhits 0 time 264 pv d8h4
 info depth 95 seldepth 2 multipv 1 score mate 1 nodes 3045 nps 11534 tbhits 0 time 264 pv d8h4
 info depth 96 seldepth 2 multipv 1 score mate 1 nodes 3077 nps 11655 tbhits 0 time 264 pv d8h4
 info depth 97 seldepth 2 multipv 1 score mate 1 nodes 3109 nps 10947 tbhits 0 time 284 pv d8h4
 info depth 98 seldepth 2 multipv 1 score mate 1 nodes 3141 nps 11059 tbhits 0 time 284 pv d8h4
 info depth 99 seldepth 2 multipv 1 score mate 1 nodes 3173 nps 10792 tbhits 0 time 294 pv d8h4
 info depth 100 seldepth 2 multipv 1 score mate 1 nodes 3205 nps 10901 tbhits 0 time 294 pv d8h4
 info depth 101 seldepth 2 multipv 1 score mate 1 nodes 3237 nps 11010 tbhits 0 time 294 pv d8h4
 info depth 102 seldepth 2 multipv 1 score mate 1 nodes 3269 nps 10444 tbhits 0 time 313 pv d8h4
 info depth 103 seldepth 2 multipv 1 score mate 1 nodes 3301 nps 10546 tbhits 0 time 313 pv d8h4
 info depth 104 seldepth 2 multipv 1 score mate 1 nodes 3333 nps 10648 tbhits 0 time 313 pv d8h4
 info depth 105 seldepth 2 multipv 1 score mate 1 nodes 3365 nps 10353 tbhits 0 time 325 pv d8h4
 info depth 106 seldepth 2 multipv 1 score mate 1 nodes 3397 nps 10452 tbhits 0 time 325 pv d8h4
 info depth 107 seldepth 2 multipv 1 score mate 1 nodes 3429 nps 9997 tbhits 0 time 343 pv d8h4
 info depth 108 seldepth 2 multipv 1 score mate 1 nodes 3461 nps 10090 tbhits 0 time 343 pv d8h4
 info depth 109 seldepth 2 multipv 1 score mate 1 nodes 3493 nps 9784 tbhits 0 time 357 pv d8h4
 info depth 110 seldepth 2 multipv 1 score mate 1 nodes 3525 nps 9873 tbhits 0 time 357 pv d8h4
 info depth 111 seldepth 2 multipv 1 score mate 1 nodes 3557 nps 9536 tbhits 0 time 373 pv d8h4
 info depth 112 seldepth 2 multipv 1 score mate 1 nodes 3589 nps 9621 tbhits 0 time 373 pv d8h4
 info depth 113 seldepth 2 multipv 1 score mate 1 nodes 3621 nps 9707 tbhits 0 time 373 pv d8h4
 info depth 114 seldepth 2 multipv 1 score mate 1 nodes 3653 nps 9390 tbhits 0 time 389 pv d8h4
 info depth 115 seldepth 2 multipv 1 score mate 1 nodes 3685 nps 9473 tbhits 0 time 389 pv d8h4
 info depth 116 seldepth 2 multipv 1 score mate 1 nodes 3717 nps 9555 tbhits 0 time 389 pv d8h4
 info depth 117 seldepth 2 multipv 1 score mate 1 nodes 3749 nps 9211 tbhits 0 time 407 pv d8h4
 info depth 118 seldepth 2 multipv 1 score mate 1 nodes 3781 nps 9289 tbhits 0 time 407 pv d8h4
 info depth 119 seldepth 2 multipv 1 score mate 1 nodes 3813 nps 9078 tbhits 0 time 420 pv d8h4
 info depth 120 seldepth 2 multipv 1 score mate 1 nodes 3845 nps 9154 tbhits 0 time 420 pv d8h4
 info depth 121 seldepth 2 multipv 1 score mate 1 nodes 3877 nps 8912 tbhits 0 time 435 pv d8h4
 info depth 122 seldepth 2 multipv 1 score mate 1 nodes 3909 nps 8986 tbhits 0 time 435 pv d8h4
 info depth 123 seldepth 2 multipv 1 score mate 1 nodes 3941 nps 8719 tbhits 0 time 452 pv d8h4
 info depth 124 seldepth 2 multipv 1 score mate 1 nodes 3973 nps 8789 tbhits 0 time 452 pv d8h4
 info depth 125 seldepth 2 multipv 1 score mate 1 nodes 4005 nps 8860 tbhits 0 time 452 pv d8h4
 info depth 126 seldepth 2 multipv 1 score mate 1 nodes 4037 nps 8589 tbhits 0 time 470 pv d8h4
 info depth 127 seldepth 2 multipv 1 score mate 1 nodes 4069 nps 8441 tbhits 0 time 482 pv d8h4
 bestmove d8h4";
            #endregion
            var historyEntry = new HistoryEntry
            {
                GameId = 10,
                MoveNumber = 2,
                Fen = "rnbqkbnr/pppp1ppp/8/4p3/5PP1/8/PPPPP2P/RNBQKBNR b KQkq G3 0",
                Move = "g4"
            };

            var mock = GetFakeHistoryRepo(historyEntry);
            var processor = new AnalysisSimplifier(mock);
            var boardAnalysis = new BoardAnalysis
            {
                BoardEvaluation = 1,
                Analysis = data,
                BoardEvaluationType = EvaluationType.MateInN
            };

            var expectedAnalysis = new BoardAnalysis
            {
                BoardEvaluation = 1,
                Analysis = "Qh4#",
                BoardEvaluationType = EvaluationType.MateInN
            };

            var newAnalysis = processor.ProcessBoardAnalysis(10, 2, boardAnalysis);
            Assert.AreEqual(expectedAnalysis, newAnalysis, "Expected analysis text to have move substituted in");
            mock.VerifyAllExpectations();
        }
    }
}
