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
        private GameDto GetFakeGame()
        {
            var myUserProfile = new UserProfile { UserId = 23, UserName = "james" };
            var opponentUserProfile = new UserProfile { UserId = 27, UserName = "clive" };

            var fakeGame = MockRepository.GenerateStub<GameDto>();

            fakeGame.UserProfileBlack = myUserProfile;
            fakeGame.UserProfileWhite = opponentUserProfile;
            fakeGame.UserProfileWinner = myUserProfile;
            fakeGame.UserIdWhite = opponentUserProfile.UserId;
            fakeGame.UserIdBlack = myUserProfile.UserId;
            fakeGame.GameOver = true;
            return fakeGame;
        }

        [Test]
        public void PgnWriting()
        {
            const int gameId = 10;

            var historyEntries = new[]
            {
                new HistoryEntry() { Fen = "rnbqkbnr/pppppppp/8/8/8/8/PPPPPPPP/RNBQKBNR w KQkq - 0", GameId = gameId, HistoryId = 0, Move = "", MoveNumber = 1},
                new HistoryEntry() { Fen = "rnbqkbnr/pppp1ppp/8/413/8/8/PPPPPPPP/RNBQKBNR w KQkq - 0", GameId = gameId, HistoryId = 1, Move = "e4", MoveNumber = 2}
            };

            var gameRepo = MockRepository.GenerateMock<IGameRepository>();
            gameRepo.Expect(x => x.FindById(gameId)).Return(GetFakeGame());

            var historyRepo = MockRepository.GenerateMock<IHistoryRepository>();
            historyRepo.Expect(x => x.FindAllMoves(10)).Return(historyEntries);

            var clockRepo = MockRepository.GenerateMock<IClockRepository>();

            var gameManager = new GameManager(gameRepo, historyRepo, clockRepo);
            var controller = new HistoryController(gameManager);
            var pgn = controller.Pgn(gameId).Content;

            Assert.AreEqual("[Event \"Casual Game\"]\r\n[Site \"?\"]\r\n[Round \"?\"]\r\n[Date \"0001.01.01\"]\r\n[White \"clive\"]\r\n[Black \"james\"]\r\n[Result \"0-1\"]\r\n\r\n1. e4 0-1", pgn, "PGN of game not as expected");
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
            const int gameId = 10;

            var historyEntries = new[]
            {
                new HistoryEntry()
                {
                    Fen = "rnbqkbnr/pppppppp/8/8/8/8/PPPPPPPP/RNBQKBNR w KQkq - 0",
                    GameId = gameId,
                    HistoryId = 0,
                    Move = "",
                    MoveNumber = 1
                },
                new HistoryEntry()
                {
                    Fen = "rnbqkbnr/pppp1ppp/8/413/8/8/PPPPPPPP/RNBQKBNR w KQkq - 0",
                    GameId = gameId,
                    HistoryId = 1,
                    Move = "e4",
                    MoveNumber = 2
                }
            };

            var gameRepo = MockRepository.GenerateMock<IGameRepository>();
            gameRepo.Expect(x => x.FindById(gameId)).Return(GetFakeGame());
            var historyRepo = MockRepository.GenerateMock<IHistoryRepository>();
            historyRepo.Expect(x => x.FindAllMoves(gameId)).Return(historyEntries);
            var gameManager = new GameManager(gameRepo, historyRepo);
            var controller = new HistoryController(gameManager);

            var result = controller.ShowMove(gameId) as ViewResult;
            Assert.AreEqual(result.ViewData.Model, gameId, "Model for ShowMove is incorrect");
        }

        [Test]
        public void ShowMoveReturnsCorrectViewWhenGameNotPresent()
        {
            const int gameId = 10;

            var gameRepo = MockRepository.GenerateMock<IGameRepository>();
            gameRepo.Expect(x => x.FindById(gameId)).Return(null);
            var historyRepo = MockRepository.GenerateMock<IHistoryRepository>();
            historyRepo.Expect(x => x.FindAllMoves(gameId)).Return(Enumerable.Empty<HistoryEntry>());
            var gameManager = new GameManager(gameRepo, historyRepo);
            var controller = new HistoryController(gameManager);

            var actionResult = controller.ShowMove(gameId);
            Assert.IsInstanceOf(typeof(HttpNotFoundResult), actionResult, "Expected 404 when requesting history of non-existent game");
        }

        [TestCase("1", "11", Description = "Non-existent game")]
        [TestCase("7", "10", Description = "Non-existent move")]
        public void PlayFromHereNonExistentData(string move, string gameId)
        {
            const int game = 10;
            var historyEntries = new[]
{
                new HistoryEntry() { Fen = "rnbqkbnr/pppppppp/8/8/8/8/PPPPPPPP/RNBQKBNR w KQkq - 0", GameId = game, HistoryId = 0, Move = "", MoveNumber = 1},
                new HistoryEntry() { Fen = "rnbqkbnr/pppp1ppp/8/413/8/8/PPPPPPPP/RNBQKBNR w KQkq - 0", GameId = game, HistoryId = 1, Move = "e4", MoveNumber = 2}
            };

            var fakeGame = GetFakeGame();
            var gameRepo = MockRepository.GenerateMock<IGameRepository>();
            gameRepo.Expect(x => x.FindById(game)).Return(fakeGame);
            var historyRepo = MockRepository.GenerateMock<IHistoryRepository>();
            historyRepo.Expect(x => x.FindAllMoves(game)).Return(historyEntries);
            var gameManager = new GameManager(gameRepo, historyRepo);

            var identityProvider = MockRepository.GenerateStub<ICurrentUser>();
            identityProvider.Expect(x => x.CurrentUserId).Return(fakeGame.UserIdWhite);

            var controller = new HistoryController(gameManager, identityProvider);
            
            var actionResult = controller.PlayFromHere(move, gameId) as ViewResult;
            Assert.AreEqual(actionResult.ViewName, "Error", "Expected an error");
        }

        private IEnumerable<TestCaseData> Identities()
        {
            var fakeGame = GetFakeGame();
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

            var fakeGame = GetFakeGame();

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
