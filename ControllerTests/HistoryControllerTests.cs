using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using Chess.Controllers;
using NUnit.Framework;
using RedChess.WebEngine.Models;
using RedChess.WebEngine.Repositories;
using RedChess.WebEngine.Repositories.Interfaces;
using Rhino.Mocks;

namespace RedChess.ControllerTests
{
    [TestFixture]
    public class HistoryControllerTests
    {
        private IHistoryRepository GetHistoryRepo()
        {
            var historyEntries = new[]
            {
                new HistoryEntry()
                {
                    Fen = "rnbqkbnr/pppppppp/8/8/8/8/PPPPPPPP/RNBQKBNR w KQkq - 0",
                    GameId = FakeGame.DefaultGameId,
                    HistoryId = 0,
                    Move = "",
                    MoveNumber = 1
                },
                new HistoryEntry()
                {
                    Fen = "rnbqkbnr/pppp1ppp/8/413/8/8/PPPPPPPP/RNBQKBNR w KQkq - 0",
                    GameId = FakeGame.DefaultGameId,
                    HistoryId = 1,
                    Move = "e4",
                    MoveNumber = 2
                }
            };

            var historyRepo = MockRepository.GenerateMock<IHistoryRepository>();
            historyRepo.Expect(x => x.FindAllMoves(FakeGame.DefaultGameId)).Return(historyEntries);

            return historyRepo;
        }

        private IHistoryRepository GetHistoryRepoNonDefaultStart()
        {
            var historyEntries = new[]
            {
                new HistoryEntry()
                {
                    Fen = "rnbqk3/pppppppp/8/8/8/8/PPPPPPPP/4KBNR w KQkq - 0",
                    GameId = FakeGame.DefaultGameId,
                    HistoryId = 0,
                    Move = "",
                    MoveNumber = 1
                },
                new HistoryEntry()
                {
                    Fen = "rnbqkbnr/pppp1ppp/8/413/8/8/PPPPPPPP/RNBQKBNR w KQkq - 0",
                    GameId = FakeGame.DefaultGameId,
                    HistoryId = 1,
                    Move = "e4",
                    MoveNumber = 2
                }
            };

            var historyRepo = MockRepository.GenerateMock<IHistoryRepository>();
            historyRepo.Expect(x => x.FindAllMoves(FakeGame.DefaultGameId)).Return(historyEntries);

            return historyRepo;
        }

        private IEnumerable<TestCaseData>PgnWritingTestSource()
        {
            yield return new TestCaseData(new FakeGame().WhiteWins().Build(), "[Result \"1-0\"]\r\n\r\n1. e4 1-0", false, true).SetDescription("White wins");
            yield return new TestCaseData(new FakeGame().BlackWins().Build(), "[Result \"0-1\"]\r\n\r\n1. e4 0-1", false, true).SetDescription("Black wins");
            yield return new TestCaseData(new FakeGame().GameOver().Build(), "[Result \"1/2-1/2\"]\r\n\r\n1. e4 1/2-1/2", false, true).SetDescription("Drawn game");
            yield return new TestCaseData(new FakeGame().Build(), "[Result \"*\"]\r\n\r\n1. e4 *", false, true).SetDescription("Incomplete game");
            yield return new TestCaseData(new FakeGame().Build(), "[Result \"*\"]\r\n[TimeControl \"300\"]\r\n\r\n1. e4 *", true, true).SetDescription("Timed game");

            yield return new TestCaseData(new FakeGame().Build(), 
                "[Result \"*\"]\r\n[FEN \"rnbqk3/pppppppp/8/8/8/8/PPPPPPPP/4KBNR w KQkq - 0\"]\r\n\r\n1. e4 *", 
                false, 
                false)
                .SetDescription("Non-default start position");
        }

        [TestCaseSource(nameof(PgnWritingTestSource))]
        public void PgnWriting(GameDto inputGame, string expectedResultSuffix, bool timedGame = false, bool defaultStartPosition = true)
        {
            const string pgnPrefix = "[Event \"Casual Game\"]\r\n[Site \"?\"]\r\n[Round \"?\"]\r\n[Date \"0001.01.01\"]\r\n[White \"clive\"]\r\n[Black \"james\"]\r\n";
            var gameRepo = FakeGame.MockRepoForGame(inputGame);

            var historyRepo = defaultStartPosition ? GetHistoryRepo() : GetHistoryRepoNonDefaultStart();
            var clockRepo = MockRepository.GenerateMock<IClockRepository>();

            if (timedGame)
            {
                var stubClock = MockRepository.GenerateStub<IClock>();
                stubClock.TimeLimitMs = 300000; // 5 minutes
                clockRepo.Expect(x => x.Clock(inputGame.GameId)).Return(stubClock);
            }

            var gameManager = new GameManager(gameRepo, historyRepo, clockRepo);
            var controller = new HistoryController(gameManager);
            var pgn = controller.Pgn(FakeGame.DefaultGameId).Content;

            Assert.AreEqual(pgnPrefix + expectedResultSuffix, pgn, "PGN of game not as expected");
        }

        [Test]
        public void NoIndexPage()
        {
            var controller = new HistoryController();
            var actionResult = controller.Index();
            Assert.IsInstanceOf(typeof(HttpNotFoundResult), actionResult, "Expected 404 when requesting history index page");
        }

        [Test]
        public void ShowMoveReturnsCorrectView()
        {
            var gameRepo = FakeGame.MockRepoForDefaultFakeGame();
            var historyRepo = GetHistoryRepo();
            var gameManager = new GameManager(gameRepo, historyRepo);
            var controller = new HistoryController(gameManager);

            var result = controller.ShowMove(FakeGame.DefaultGameId) as ViewResult;
            Assert.AreEqual(result.ViewData.Model, FakeGame.DefaultGameId, "Model for ShowMove is incorrect");
        }

        [Test]
        public void ShowMoveReturnsCorrectViewWhenGameNotPresent()
        {
            var gameRepo = MockRepository.GenerateMock<IGameRepository>();
            gameRepo.Expect(x => x.FindById(FakeGame.DefaultGameId)).Return(null);
            var historyRepo = MockRepository.GenerateMock<IHistoryRepository>();
            historyRepo.Expect(x => x.FindAllMoves(FakeGame.DefaultGameId)).Return(Enumerable.Empty<HistoryEntry>());
            var gameManager = new GameManager(gameRepo, historyRepo);
            var controller = new HistoryController(gameManager);

            var actionResult = controller.ShowMove(FakeGame.DefaultGameId);
            Assert.IsInstanceOf(typeof(HttpNotFoundResult), actionResult, "Expected 404 when requesting history of non-existent game");
        }

        [TestCase("1", "11", Description = "Non-existent game")]
        [TestCase("7", "10", Description = "Non-existent move")]
        public void PlayFromHereNonExistentData(string move, string gameId)
        {
            GameDto fakeGame = new FakeGame();
            var gameRepo = FakeGame.MockRepoForGame(fakeGame);
            var historyRepo = GetHistoryRepo();
            var gameManager = new GameManager(gameRepo, historyRepo);

            var identityProvider = MockRepository.GenerateStub<ICurrentUser>();
            identityProvider.Expect(x => x.CurrentUserId).Return(fakeGame.UserIdWhite);

            var controller = new HistoryController(gameManager, identityProvider);
            
            var actionResult = controller.PlayFromHere(move, gameId) as ViewResult;
            Assert.AreEqual(actionResult.ViewName, "Error", "Expected an error");
        }

        private IEnumerable<TestCaseData> Identities()
        {
            GameDto fakeGame = new FakeGame();
            yield return new TestCaseData(IdentityProviderForFakeGameWhite(fakeGame)).SetName("Play as white");
            yield return new TestCaseData(IdentityProviderForFakeGameBlack(fakeGame)).SetName("Play as black");
        }

        [TestCaseSource(nameof(Identities))]
        public void PlayFromHereSuccessfully(ICurrentUser identityProvider)
        {
            const int gameId = 10;
            const int clonedGameId = 99; // This would come from the DB in reality
            var historyEntries = new[]
{
                new HistoryEntry() { Fen = "rnbqkbnr/pppppppp/8/8/8/8/PPPPPPPP/RNBQKBNR w KQkq - 0", GameId = gameId, HistoryId = 0, Move = "", MoveNumber = 1},
                new HistoryEntry() { Fen = "rnbqkbnr/pppp1ppp/8/413/8/8/PPPPPPPP/RNBQKBNR w KQkq - 0", GameId = gameId, HistoryId = 1, Move = "e4", MoveNumber = 2}
            };

            GameDto fakeGame = new FakeGame();

            var gameRepo = MockRepository.GenerateStub<IGameRepository>();
            gameRepo.Expect(x => x.FindById(gameId)).Return(fakeGame);
            gameRepo.Expect(x => x.AddOrUpdate(Arg<GameDto>.Is.Anything)).Do(new Action<GameDto>((dto) =>
            {
                dto.GameId = clonedGameId;
            }));

            var historyRepo = MockRepository.GenerateMock<IHistoryRepository>();
            historyRepo.Expect(x => x.FindByGameIdAndMoveNumber(gameId, 1)).Return(historyEntries[0]);
            historyRepo.Expect(x => x.CloneGame(clonedGameId, gameId, 1)).Repeat.Once();

            var fakeUserProfileRepo = FakeUserProfileRepo(fakeGame);

            var fakeAnalysisRepo = MockRepository.GenerateMock<IAnalysisRepository>();
            fakeAnalysisRepo.Expect(x => x.CloneGame(clonedGameId, gameId, 1)).Repeat.Once();

            var gameManager = new GameManager(gameRepo, historyRepo, userProfileRepository: fakeUserProfileRepo, analysisRepository:fakeAnalysisRepo);
            var controller = new HistoryController(gameManager, identityProvider);

            var actionResult = controller.PlayFromHere("1", "10") as RedirectToRouteResult;
            historyRepo.VerifyAllExpectations();
            fakeAnalysisRepo.VerifyAllExpectations();

            Assert.AreEqual(actionResult.RouteValues["id"], clonedGameId, "Id of cloned game is not as expected");
            Assert.AreEqual(actionResult.RouteValues["action"], "Details", "Expected action of cloned game to be Details");
            Assert.AreEqual(actionResult.RouteValues["controller"], "Board", "Expected controller of cloned game to be Board");
        }

        private static ICurrentUser IdentityProviderForFakeGameWhite(GameDto fakeGame)
        {
            var identityProvider = MockRepository.GenerateStub<ICurrentUser>();
            identityProvider.Expect(x => x.CurrentUserId).Return(fakeGame.UserIdWhite);
            identityProvider.Expect(x => x.CurrentUser).Return(fakeGame.UserProfileWhite.UserName);
            return identityProvider;
        }
        private static ICurrentUser IdentityProviderForFakeGameBlack(GameDto fakeGame)
        {
            var identityProvider = MockRepository.GenerateStub<ICurrentUser>();
            identityProvider.Expect(x => x.CurrentUserId).Return(fakeGame.UserIdBlack);
            identityProvider.Expect(x => x.CurrentUser).Return(fakeGame.UserProfileBlack.UserName);
            return identityProvider;
        }

        private static IUserProfileRepository FakeUserProfileRepo(GameDto fakeGame)
        {
            var fakeUserProfileRepo = MockRepository.GenerateMock<IUserProfileRepository>();
            fakeUserProfileRepo.Expect(x => x.UserId(fakeGame.UserProfileWhite.UserName)).Return(fakeGame.UserIdWhite);
            fakeUserProfileRepo.Expect(x => x.UserId(fakeGame.UserProfileBlack.UserName)).Return(fakeGame.UserIdBlack);
            return fakeUserProfileRepo;
        }
    }
}
