﻿using System;
using System.Collections.Specialized;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;
using Chess.Controllers;
using Chess.Filters;
using NUnit.Framework;
using RedChess.ChessCommon.Enumerations;
using RedChess.MessageQueue;
using RedChess.WebEngine.Models;
using RedChess.WebEngine.Repositories;
using RedChess.WebEngine.Repositories.Interfaces;
using Rhino.Mocks;

namespace RedChess.ControllerTests
{
    [TestFixture]
    class BoardControllerTests
    {
        private int m_moveNumber = 0;

        private GameDto GetFakeGamePromotionImminent(int id)
        {
            return new FakeGame().WithId(id).WithFen("8/P7/8/8/8/8/8/K6k w - - 0");
        }

        private IGameRepository MockRepoForDefaultFakeGame()
        {
            var repository = MockRepository.GenerateMock<IGameRepository>();
            repository.Expect(x => x.FindById(FakeGame.DefaultGameId)).Return(new FakeGame());
            return repository;
        }

        private BoardController GetControllerForFakeEndedGameAsUser(string userName, out IGameRepository repository)
        {
            GameDto fakeGame = new FakeGame().GameOver();

            var fakeHistoryRepo = MockRepository.GenerateMock<IHistoryRepository>();
            var fakeClockRepo = MockRepository.GenerateMock<IClockRepository>();

            repository = MockRepoForGame(fakeGame);
            var fakeIdentity = MockRepository.GenerateStub<ICurrentUser>();
            fakeIdentity.Stub(x => x.CurrentUser).Return(userName);

            var manager = new GameManager(repository, fakeHistoryRepo, fakeClockRepo);
            return new BoardController(manager, fakeIdentity);
        }

        private IGameRepository MockRepoForGame(GameDto fakeGame)
        {
            var repository = MockRepository.GenerateMock<IGameRepository>();
            repository.Expect(x => x.FindById(fakeGame.GameId)).Return(fakeGame);
            return repository;
        }

        private BoardController GetControllerForFakeGameWithQueue(out IQueueManager fakeQueueManager, bool expectPostMessage)
        {
            GameDto fakeGame = new FakeGame();

            var fakeHistoryRepo = MockRepository.GenerateMock<IHistoryRepository>();
            var fakeClockRepo = MockRepository.GenerateMock<IClockRepository>();

            var repository = MockRepoForDefaultFakeGame();
            var fakeIdentity = MockRepository.GenerateStub<ICurrentUser>();
            fakeIdentity.Stub(x => x.CurrentUser).Return("clive");
            fakeQueueManager = MockRepository.GenerateMock<IQueueManager>();
            if (expectPostMessage)
            {
                fakeQueueManager.Expect(x => x.PostRequestBestMoveMessage(10, 0, fakeGame.Fen, "e2e4")).Repeat.Once();
            }
            else
            {
                fakeQueueManager.Expect(x => x.PostRequestBestMoveMessage(Arg<int>.Is.Anything, Arg<int>.Is.Anything, Arg<string>.Is.Anything, Arg<string>.Is.Anything)).Repeat.Never();
            }

            var manager = new GameManager(repository, fakeHistoryRepo, fakeClockRepo, fakeQueueManager);
            return new BoardController(manager, fakeIdentity);
        }

        private IGameManager GetGameManagerForFakeGameWithQueueExpectingGameOver(int gameId, out IQueueManager fakeQueueManager, AutoResetEvent autoResetEvent)
        {
            GameDto fakeGame = new FakeGame().WithId(gameId);

            var fakeHistoryRepo = MockRepository.GenerateMock<IHistoryRepository>();
            var fakeClockRepo = MockRepository.GenerateMock<IClockRepository>();

            var repository = MockRepository.GenerateMock<IGameRepository>();
            repository.Expect(x => x.FindById(gameId)).Return(fakeGame);
            var fakeIdentity = MockRepository.GenerateStub<ICurrentUser>();
            fakeIdentity.Stub(x => x.CurrentUser).Return("clive");
            fakeQueueManager = MockRepository.GenerateMock<IQueueManager>();
            fakeQueueManager.Expect(x => x.PostGameEndedMessage(gameId)).Repeat.Once().Do(new Action<int>(id =>
            {
                autoResetEvent.Set();
            }));

            return new GameManager(repository, fakeHistoryRepo, fakeClockRepo, fakeQueueManager);
        }

        private BoardController GetControllerForFakeGameWithQueueExpectingGameOver(IGameManager manager)
        {
            var fakeIdentity = MockRepository.GenerateStub<ICurrentUser>();
            fakeIdentity.Stub(x => x.CurrentUser).Return("clive");
            return new BoardController(manager, fakeIdentity);
        }


        private BoardController GetControllerForFakeGameWithQueueWithPromotionImminent(out IQueueManager fakeQueueManager)
        {
            var fakeGame = GetFakeGamePromotionImminent(FakeGame.DefaultGameId);

            var fakeHistoryRepo = MockRepository.GenerateMock<IHistoryRepository>();
            var fakeClockRepo = MockRepository.GenerateMock<IClockRepository>();

            var repository = MockRepository.GenerateMock<IGameRepository>();
            repository.Expect(x => x.FindById(FakeGame.DefaultGameId)).Return(fakeGame);
            var fakeIdentity = MockRepository.GenerateStub<ICurrentUser>();
            fakeIdentity.Stub(x => x.CurrentUser).Return("clive");
            fakeQueueManager = MockRepository.GenerateMock<IQueueManager>();
            fakeQueueManager.Expect(x => x.PostRequestBestMoveMessage(10, 0, fakeGame.Fen, "a7a8q")).Repeat.Once();

            var manager = new GameManager(repository, fakeHistoryRepo, fakeClockRepo, fakeQueueManager);
            return new BoardController(manager, fakeIdentity);
        }

        private GameDto GetFakeGameAboutToPromote()
        {
            return new FakeGame().WithFen("k6K/8/8/8/8/8/7p/8 b - - 0");
        }

        [Test]
        public void GetDetailsCallsFindById()
        {
            GameDto fakeGame = new FakeGame().WithId(40);
            var fakeRepo = MockRepoForGame(fakeGame);
            fakeRepo.Replay();
            var manager = new GameManager(fakeRepo);
            var controller = new BoardController(manager);
            controller.Details(40);
           
            fakeRepo.VerifyAllExpectations();
        }

        [Test]
        public void MessagePostedToQueueOnMove()
        {
            IQueueManager fakeQueueManager;
            var fakeController = GetControllerForFakeGameWithQueue(out fakeQueueManager, expectPostMessage:true);
            fakeController.PlayMove(10, "E2", "E4", "");
            fakeQueueManager.VerifyAllExpectations();
        }

        [Test]
        public void MessagePostedToQueueOnPromotion()
        {
            IQueueManager fakeQueueManager;
            var fakeController = GetControllerForFakeGameWithQueueWithPromotionImminent(out fakeQueueManager);
            fakeController.PlayMove(10, "A7", "A8", "Queen");
            fakeQueueManager.VerifyAllExpectations();
        }

        [Test]
        public void NoMessagePostedToQueueOnBadMove()
        {
            IQueueManager fakeQueueManager;
            var fakeController = GetControllerForFakeGameWithQueue(out fakeQueueManager, expectPostMessage: false);
            fakeController.PlayMove(10, "E2", "E5", "");
            fakeQueueManager.VerifyAllExpectations();
        }

        [Test]
        public void DeleteConfirmedCallsDelete()
        {
            var fakeIdProvider = MockRepository.GenerateStub<ICurrentUser>();
            fakeIdProvider.Expect(x => x.CurrentUser).Return("james");

            var fakeRepo = MockRepository.GenerateMock<IGameRepository>();
            fakeRepo.Expect(x => x.Delete(40));
            var fakeStatsRepo = MockRepository.GenerateMock<IStatsRepository>();

            var manager = new GameManager(fakeRepo, null, null, null, null, null, fakeStatsRepo);
            var controller = new BoardController(manager, fakeIdProvider);
            controller.DeleteConfirmed(40);

            fakeRepo.VerifyAllExpectations();
        }

        [Test]
        public void DeleteMultipleCallsDelete()
        {
            var fakeRepo = MockRepository.GenerateStrictMock<IGameRepository>();
            var fakeStatsRepo = MockRepository.GenerateMock<IStatsRepository>();

            fakeRepo.Expect(x => x.Delete(FakeGame.DefaultGameId));
            fakeRepo.Expect(x => x.Delete(20));
            fakeRepo.Expect(x => x.Delete(30));
            fakeRepo.Expect(x => x.Delete(40));
            var manager = new GameManager(fakeRepo, null, null, null, null, null, fakeStatsRepo);
            var fakeIdentity = MockRepository.GenerateStub<ICurrentUser>();
            fakeIdentity.Stub(x => x.CurrentUser).Return("james");
            var controller = new BoardController(manager, fakeIdentity);
            controller.DeleteMultiple(FakeGame.DefaultGameId + ",20,30,40");

            fakeRepo.VerifyAllExpectations();
        }

        [Test]
        public void DeleteMultipleCannotDeleteArbitraryGames()
        {
            var fakeRepo = MockRepository.GenerateStrictMock<IGameRepository>();
            fakeRepo.Expect(x => x.FindById(FakeGame.DefaultGameId)).Return(new FakeGame());
            fakeRepo.AssertWasNotCalled(x => x.Delete(FakeGame.DefaultGameId));
            var manager = new GameManager(fakeRepo);
            var fakeIdentity = MockRepository.GenerateStub<ICurrentUser>();
            fakeIdentity.Stub(x => x.CurrentUser).Return("captain_bogus");
            var controller = new BoardController(manager, fakeIdentity);
            var actionAllowedByFilter = PerformParticipantFiltering(controller, manager, fakeIdentity, "DeleteMultiple");
            Assert.IsFalse(actionAllowedByFilter, "Filter should have denied us");
            fakeRepo.VerifyAllExpectations();
        }

        [Test]
        public void DeleteMultipleOneArgumentCallsDelete()
        {
            var fakeRepo = MockRepository.GenerateStrictMock<IGameRepository>();
            var fakeStatsRepo = MockRepository.GenerateMock<IStatsRepository>();
            fakeRepo.Expect(x => x.Delete(FakeGame.DefaultGameId));

            var manager = new GameManager(fakeRepo, null, null, null, null, null, fakeStatsRepo);

            var fakeIdentity = MockRepository.GenerateStub<ICurrentUser>();
            fakeIdentity.Stub(x => x.CurrentUser).Return("james");
            var controller = new BoardController(manager, fakeIdentity);
            controller.DeleteMultiple(FakeGame.DefaultGameId.ToString());

            fakeRepo.VerifyAllExpectations();
        }

        [TestCase("james", true)]
        [TestCase("clive", true)]
        [TestCase("jason", false)]
        public void MayManipulateBoardTest(string userName, bool expectedResult)
        {
            var fakeRepo = MockRepoForDefaultFakeGame();
            var manager = new GameManager(fakeRepo);
            var identityProvider = MockRepository.GenerateMock<ICurrentUser>();
            identityProvider.Expect(x => x.CurrentUser).Return(userName);
            var accessValidator = new AccessValidator(manager, identityProvider);
            bool ok = accessValidator.MayAccess(FakeGame.DefaultGameId);
            fakeRepo.VerifyAllExpectations();
            Assert.AreEqual(expectedResult, ok,"Permission to use board was not as expected");
        }

        private BoardController GetControllerForFakeGameAsUser(string userName, out IGameRepository repository)
        {
            var fakeHistoryRepo = MockRepository.GenerateMock<IHistoryRepository>();
            var fakeClockRepo = MockRepository.GenerateMock<IClockRepository>();
            var fakeStatsRepo = MockRepository.GenerateMock<IStatsRepository>();

            repository = MockRepoForDefaultFakeGame();
            var fakeIdentity = MockRepository.GenerateStub<ICurrentUser>();
            fakeIdentity.Stub(x => x.CurrentUser).Return(userName);

            var manager = new GameManager(repository, fakeHistoryRepo, fakeClockRepo, null, null, null, fakeStatsRepo);
            return new BoardController(manager, fakeIdentity);
        }

        private ClockController GetClockControllerForFakeGameAsUser(ICurrentUser identity, out IGameRepository repository)
        {
            var fakeHistoryRepo = MockRepository.GenerateMock<IHistoryRepository>();
            var fakeClockRepo = MockRepository.GenerateMock<IClockRepository>();

            repository = MockRepoForDefaultFakeGame();

            var manager = new GameManager(repository, fakeHistoryRepo, fakeClockRepo);
            return new ClockController(manager, identity);
        }

        private BoardController GetControllerForFakeGameAsUserWithClock(string userName, out IGameRepository repository)
        {
            var fakeHistoryRepo = MockRepository.GenerateMock<IHistoryRepository>();
            var fakeClockRepo = MockRepository.GenerateMock<IClockRepository>();
            var fakeStatsRepo = MockRepository.GenerateMock<IStatsRepository>();

            repository = MockRepoForDefaultFakeGame();
            var fakeIdentity = MockRepository.GenerateStub<ICurrentUser>();
            fakeIdentity.Stub(x => x.CurrentUser).Return(userName);
            fakeClockRepo.Expect(x => x.Clock(FakeGame.DefaultGameId)).Return(new Clock());

            var manager = new GameManager(repository, fakeHistoryRepo, fakeClockRepo, null, null, null, fakeStatsRepo);
            return new BoardController(manager, fakeIdentity);
        }

        [TestCase(false, "FAIL")]
        [TestCase(true, "DRAW")]
        public void ClaimDrawReturnsCorrectStatus(bool drawClaimable, string expectedStatus)
        {
            GameDto fakeGame = new FakeGame().WithClaimableDraw(drawClaimable);
            var manager = MockRepository.GenerateStub<IGameManager>();
            manager.Expect(x => x.FetchGame(FakeGame.DefaultGameId)).Return(new GameBinding(fakeGame, manager));
            var controller = new BoardController(manager);
            var result = controller.ClaimDraw(FakeGame.DefaultGameId) as JsonResult;
            var status = PropertyUtils.ExtractPropertyValue<string>(result, "status");
            Assert.AreEqual(expectedStatus, status, $"Expected {expectedStatus} status when claiming draw");
        }

        [TestCase("0.99")]
        [TestCase("180.1")]
        [TestCase("-5")]
        [TestCase("Not a number")]
        [TestCase("")]
        [TestCase(null)]
        public void CreateGameInvalidTimeLimits(string timeLimit)
        {
            const int whiteUserId = 44;

            var fakeGameRepo = MockRepository.GenerateMock<IGameRepository>();
            fakeGameRepo.Expect(x => x.AddOrUpdate(Arg<GameDto>.Is.Anything)).Repeat.Never();

            var fakeUserRepo = MockRepository.GenerateStub<IUserProfileRepository>();
            fakeUserRepo.Expect(x => x.UserId("james")).Return(whiteUserId);

            var fakeHistoryRepo = MockRepository.GenerateStub<IHistoryRepository>();

            var manager = new GameManager(fakeGameRepo, fakeHistoryRepo, userProfileRepository: fakeUserRepo);

            var fakeIdentity = MockRepository.GenerateStub<ICurrentUser>();
            fakeIdentity.Stub(x => x.CurrentUser).Return("james");
            var controller = new BoardController(manager, fakeIdentity);

            var result = controller.Create("77", timeLimit, true) as JsonResult;
            fakeGameRepo.VerifyAllExpectations();

            Assert.IsFalse(PropertyUtils.ExtractPropertyValue<bool>(result.Data, "success"), "Game creation should fail");
        }

        [Test]
        public void CreateGame()
        {
            const int whiteUserId = 44;
            const int blackUserId = 77;

            GameDto actualGameAdded = null;
            
            var fakeGameRepo = MockRepository.GenerateMock<IGameRepository>();
            fakeGameRepo.Expect(x => x.AddOrUpdate(Arg<GameDto>.Is.Anything)).Repeat.Once().Do(new Action<GameDto>(
                x =>
                {
                    actualGameAdded = x;
                }));

            var fakeUserRepo = MockRepository.GenerateStub<IUserProfileRepository>();
            fakeUserRepo.Expect(x => x.UserId("james")).Return(whiteUserId);

            var fakeHistoryRepo = MockRepository.GenerateStub<IHistoryRepository>();

            var manager = new GameManager(fakeGameRepo, fakeHistoryRepo, userProfileRepository: fakeUserRepo);

            var fakeIdentity = MockRepository.GenerateStub<ICurrentUser>();
            fakeIdentity.Stub(x => x.CurrentUser).Return("james");
            var controller = new BoardController(manager, fakeIdentity);

            controller.Create("77", "0");
            fakeGameRepo.VerifyAllExpectations();

            Assert.AreEqual(whiteUserId, actualGameAdded.UserIdWhite, "Added game has wrong white player");
            Assert.AreEqual(blackUserId, actualGameAdded.UserIdBlack, "Added game has wrong black player");
            Assert.AreEqual("rnbqkbnr/pppppppp/8/8/8/8/PPPPPPPP/RNBQKBNR w KQkq - 0", actualGameAdded.Fen, "Starting FEN is wrong");
        }

        [TestCase("james", true)]
        [TestCase("clive", true)]
        [TestCase("jason", false)]
        public void CannotStartClockOnOtherUsersGames(string userName, bool allowed)
        {
            IGameRepository fakeRepo;
            var fakeIdentity = MockRepository.GenerateStub<ICurrentUser>();
            fakeIdentity.Stub(x => x.CurrentUser).Return(userName);
            var controller = GetClockControllerForFakeGameAsUser(fakeIdentity, out fakeRepo);
            var manager = new GameManager(fakeRepo);
            var actionAllowedByFilter = PerformParticipantFiltering(controller, manager, fakeIdentity, "PlayerReady");

            if (allowed)
            {
                Assert.IsTrue(actionAllowedByFilter, "Filter should not have denied us");
            }
            else
            {
                Assert.IsFalse(actionAllowedByFilter, "Filter should have denied us");
            }

            fakeRepo.VerifyAllExpectations();
        }

        [TestCase("james", true)]
        [TestCase("clive", true)]
        [TestCase("jason", false)]
        public void CannotDeleteOtherUsersGames(string userName, bool allowed)
        {
            IGameRepository fakeRepo;
            var controller = GetControllerForFakeGameAsUser(userName, out fakeRepo);
            var fakeIdentity = MockRepository.GenerateStub<ICurrentUser>();
            fakeIdentity.Stub(x => x.CurrentUser).Return(userName);
            var manager = new GameManager(fakeRepo);
            var actionAllowedByFilter = PerformParticipantFiltering(controller, manager, fakeIdentity, "DeleteConfirmed");

            if (allowed)
            {
                Assert.IsTrue(actionAllowedByFilter, "Filter should not have denied us");
                fakeRepo.Expect(x => x.Delete(FakeGame.DefaultGameId)).Repeat.Once();
                controller.DeleteConfirmed(FakeGame.DefaultGameId);
            }
            else
            {
                Assert.IsFalse(actionAllowedByFilter, "Filter should have denied us");
            }

            fakeRepo.VerifyAllExpectations();
        }

        /// <summary>
        /// Return true if the action is allowed, false otherwise
        /// </summary>
        /// <param name="controller"></param>
        /// <param name="manager"></param>
        /// <param name="currentUser"></param>
        /// <param name="actionName"></param>
        /// <returns></returns>
        private static bool PerformParticipantFiltering(Controller controller, IGameManager manager, ICurrentUser currentUser, string actionName)
        {
            var httpContext = MockRepository.GenerateMock<HttpContextBase>();
            var httpRequest = MockRepository.GenerateMock<HttpRequestBase>();
            var theParams = new NameValueCollection {["id"] = FakeGame.DefaultGameId.ToString()};
            httpRequest.Expect(x => x.Params).Return(theParams);
            httpContext.Expect(x => x.Request).Return(httpRequest);

            // If the attribute is missing, the action must be allowed
            var methodInfo = controller.GetType().GetMethod(actionName);
            if (methodInfo.GetCustomAttributes(typeof(VerifyIsParticipantAttribute), true).Length == 0)
                return true;

            var context =
                new AuthorizationContext(new ControllerContext(httpContext,
                    new RouteData(),
                    controller), new ReflectedActionDescriptor(methodInfo, actionName, new ReflectedControllerDescriptor(controller.GetType())));

            var authFilter = new VerifyIsParticipantAttribute(manager, currentUser);
            authFilter.OnAuthorization(context);
            return context.Result == null; // The result has not been set by the filter, so it passes
        }

        [TestCase("james", false)]
        [TestCase("clive", true)]
        [TestCase("jason", false)]
        public void CannotPlayOnOtherUsersGames(string userName, bool allowed)
        {
            IGameRepository fakeRepo;
            var controller = GetControllerForFakeGameAsUser(userName, out fakeRepo);
            var g = fakeRepo.FindById(FakeGame.DefaultGameId);

            if (allowed)
            {
                fakeRepo.Expect(x => x.AddOrUpdate(g)).Repeat.Once();
            }
            else
            {
                fakeRepo.Expect(x => x.AddOrUpdate(g)).Repeat.Never();
            }

            controller.PlayMove(FakeGame.DefaultGameId, "E2", "E4", "");
            fakeRepo.VerifyAllExpectations();
        }

        [TestCase("james", true)]
        [TestCase("clive", true)]
        [TestCase("jason", false)]
        public void CannotResignOtherUsersGames(string userName, bool allowed)
        {
            IGameRepository fakeRepo;
            var controller = GetControllerForFakeGameAsUser(userName, out fakeRepo);
            var fakeIdentity = MockRepository.GenerateStub<ICurrentUser>();
            fakeIdentity.Stub(x => x.CurrentUser).Return(userName);
            var fakeStatsRepo = MockRepository.GenerateStub<IStatsRepository>();

            var manager = new GameManager(fakeRepo,null, null, null, null, null, fakeStatsRepo);

            var actionAllowedByFilter = PerformParticipantFiltering(controller, manager, fakeIdentity, "Resign");
            var g = fakeRepo.FindById(FakeGame.DefaultGameId);

            if (allowed)
            {
                fakeRepo.Expect(x => x.AddOrUpdate(g)).Repeat.Once();
                Assert.IsTrue(actionAllowedByFilter, "Filter should not have denied us");
            }
            else
            {
                Assert.IsFalse(actionAllowedByFilter, "Filter should have denied us");
            }

            controller.Resign(FakeGame.DefaultGameId);

            var args = fakeRepo.GetArgumentsForCallsMadeOn(a => a.AddOrUpdate(g));

            if (allowed)
            {
                if (args.Count > 0)
                {
                    var dto = args[0][0] as GameDto;
                    Assert.AreEqual(true, dto.GameOver, "Expected game over state to change correctly");
                }
                else
                {
                    Assert.Fail("AddOrUpdate was not called");
                }
            }

            fakeRepo.VerifyAllExpectations();
        }

        [Test]
        public void CannotPlayOnEndedGame()
        {
            IGameRepository fakeRepo;
            var controller = GetControllerForFakeEndedGameAsUser("clive", out fakeRepo);
            fakeRepo.Expect(x => x.AddOrUpdate(null)).Repeat.Never();
            var result = controller.PlayMove(FakeGame.DefaultGameId, "E2", "E4", "");
            dynamic res = ((System.Web.Mvc.JsonResult) (result)).Data;
            Assert.AreEqual("{ fen = rnbqkbnr/pppppppp/8/8/8/8/PPPPPPPP/RNBQKBNR w KQkq - 0, message = , status = FAIL }", res.ToString(), "Result was not as expected");
            fakeRepo.VerifyAllExpectations();
        }

        [TestCase("Black is out of time", "b")]
        [TestCase("White is out of time", "w")]
        public void TimeoutEndsTheGame(string message, string timedOutColor)
        {
            IGameRepository fakeRepo;
            var controller = GetControllerForFakeGameAsUserWithClock("james", out fakeRepo);
            var g = fakeRepo.FindById(FakeGame.DefaultGameId);

            controller.TimedOut(FakeGame.DefaultGameId, message, timedOutColor);

            var args = fakeRepo.GetArgumentsForCallsMadeOn(a => a.AddOrUpdate(g));
            if (args.Count > 0)
            {
                var dto = args[0][0] as GameDto;
                Assert.IsTrue(dto.GameOver, "Expected game to be over after timeout");
                int expectedWinner = timedOutColor == "b" ? dto.UserIdWhite : dto.UserIdBlack;
                Assert.AreEqual(expectedWinner, dto.UserIdWinner, "Winner user id is incorrect");
                Assert.AreEqual(message, dto.Status, "Game status is incorrect");
            }
            else
            {
                Assert.Fail("AddOrUpdate was not called");
            }

            fakeRepo.VerifyAllExpectations();
        }

        [Test]
        public void PlayingMoveOnDeletedBoardIsError()
        {
            var fakeManager = MockRepository.GenerateStub<IGameManager>();
            fakeManager.Expect(x => x.FetchGame(FakeGame.DefaultGameId)).Return(null);
            var controller = new BoardController(fakeManager);
            var result = controller.PlayMove(FakeGame.DefaultGameId, "H2", "H1", "") as JsonResult;
            var status = PropertyUtils.ExtractPropertyValue<string>(result, "status");
            Assert.AreEqual("AUTH", status, "Expected AUTH status when playing on deleted board");
        }

        [Test]
        public void NonEmptyStatusFromMoveIsError()
        {
            const string canaryValue = "DoobyDoobyDoo";

            GameDto fakeGame = new FakeGame().WithStatus(canaryValue);
            var manager = MockRepository.GenerateStub<IGameManager>();
            manager.Expect(x => x.IsUsersTurn(FakeGame.DefaultGameId, "clive")).Return(true);
            manager.Expect(x => x.FetchGame(FakeGame.DefaultGameId)).Return(new GameBinding(fakeGame, manager));
            manager.Expect(x => x.Move(FakeGame.DefaultGameId, Location.A1, Location.A2, "")).Return(false);
            var fakeIdentity = MockRepository.GenerateStub<ICurrentUser>();
            fakeIdentity.Stub(x => x.CurrentUser).Return(fakeGame.UserProfileWhite.UserName);
            fakeIdentity.Stub(x => x.CurrentUserId).Return(fakeGame.UserProfileWhite.UserId);
            var controller = new BoardController(manager, fakeIdentity);
            var result = controller.PlayMove(FakeGame.DefaultGameId, "A1", "A2", "") as JsonResult;
            var status = PropertyUtils.ExtractPropertyValue<string>(result, "status");
            Assert.AreEqual("FAIL", status, "Expected FAIL status when playing bad move");
            var message = PropertyUtils.ExtractPropertyValue<string>(result, "message");
            StringAssert.Contains(canaryValue, message, "Expected message to contain game status");
        }

        [TestCase("I9","H8")]
        [TestCase("H8", "I0")]
        [TestCase("dgdgf", "dgffd")]
        [TestCase("", "")]
        [TestCase(null, null)]
        public void MoveDoesNotParseIsError(string start, string end)
        {
            IGameRepository repo;
            var controller = GetControllerForFakeGameAsUser("clive", out repo);
            var result = controller.PlayMove(FakeGame.DefaultGameId, start, end, "") as JsonResult;
            var status = PropertyUtils.ExtractPropertyValue<string>(result, "status");
            Assert.AreEqual("FAIL", status, $"Expected FAIL status when playing bogus moves {start}->{end}");
        }

        [Test]
        public void PlayMoveChangesBoardCorrectlyForPromotion()
        {
            var fakeGame = GetFakeGameAboutToPromote();

            var fakeRepo = MockRepository.GenerateMock<IGameRepository>();
            fakeRepo.Expect(x => x.FindById(FakeGame.DefaultGameId)).Return(fakeGame);
            var fakeHistoryRepo = MockRepository.GenerateMock<IHistoryRepository>();
            var fakeClockRepo = MockRepository.GenerateMock<IClockRepository>();
            var identityProvider = MockRepository.GenerateStub<ICurrentUser>();
            identityProvider.Expect(x => x.CurrentUser).Return(fakeGame.UserProfileBlack.UserName);
            identityProvider.Expect(x => x.CurrentUserId).Return(fakeGame.UserIdBlack);
            var manager = new GameManager(fakeRepo, fakeHistoryRepo, fakeClockRepo);
            var controller = new BoardController(manager, identityProvider);
            controller.PlayMove(FakeGame.DefaultGameId, "H2", "H1", "Q");

            var args = fakeRepo.GetArgumentsForCallsMadeOn(a => a.AddOrUpdate(fakeGame));

            fakeRepo.VerifyAllExpectations();

            var updatedDto = args[0][0] as GameDto;

            Assert.AreEqual("k6K/8/8/8/8/8/8/7q w - - 0", updatedDto.Fen, "Fen after move not as expected");
        }

        [Test]
        public void EndingAGameUpdatesTheEloTable()
        {
            var reset = new AutoResetEvent(false);
            IQueueManager fakeQueueManager;
            var gameManager = GetGameManagerForFakeGameWithQueueExpectingGameOver(FakeGame.DefaultGameId, out fakeQueueManager, reset);
            gameManager.EndGameWithMessage(FakeGame.DefaultGameId, "Ended by test");
            reset.WaitOne(TimeSpan.FromSeconds(5));
            fakeQueueManager.VerifyAllExpectations();
        }

        [Test]
        public void ResigningAGameUpdatesTheEloTable()
        {
            var reset = new AutoResetEvent(false);
            IQueueManager fakeQueueManager;
            var manager = GetGameManagerForFakeGameWithQueueExpectingGameOver(FakeGame.DefaultGameId, out fakeQueueManager, reset);
            var controller = GetControllerForFakeGameWithQueueExpectingGameOver(manager);
            controller.Resign(FakeGame.DefaultGameId);
            reset.WaitOne(TimeSpan.FromSeconds(5));
            fakeQueueManager.VerifyAllExpectations();
        }

        [Test]
        public void AcceptingADrawnGameUpdatesTheEloTable()
        {
            var reset = new AutoResetEvent(false);
            IQueueManager fakeQueueManager;
            var manager = GetGameManagerForFakeGameWithQueueExpectingGameOver(FakeGame.DefaultGameId, out fakeQueueManager, reset);
            var controller = GetControllerForFakeGameWithQueueExpectingGameOver(manager);
            // This relies on the known bug that a user can accept their own draw requests, if the UI allowed them
            controller.OfferDraw(FakeGame.DefaultGameId);
            controller.AgreeDraw(FakeGame.DefaultGameId, true);
            reset.WaitOne(TimeSpan.FromSeconds(5));
            fakeQueueManager.VerifyAllExpectations();
        }

        [Test]
        public void AgreedDrawHasCorrectStatus()
        {
            var result = PlayADraw(true);
            var status = PropertyUtils.ExtractPropertyValue<string>(result, "status");
            Assert.AreEqual("DRAW", status, "Expected status to be DRAW");
        }

        [Test]
        public void DeclinedDrawHasCorrectStatus()
        {
            var result = PlayADraw(false);
            var status = PropertyUtils.ExtractPropertyValue<string>(result, "status");
            Assert.AreEqual("REJECT", status, "Expected status to be REJECT");
        }

        private JsonResult PlayADraw(bool drawAccepted)
        {
            IGameRepository repo;
            var controller = GetControllerForFakeGameAsUser("james", out repo);
            controller.OfferDraw(FakeGame.DefaultGameId);
            return controller.AgreeDraw(FakeGame.DefaultGameId, drawAccepted) as JsonResult;
        }

        [Test]
        public void DeletingAGameUpdatesTheEloTable()
        {
            var reset = new AutoResetEvent(false);
            IQueueManager fakeQueueManager;
            var manager = GetGameManagerForFakeGameWithQueueExpectingGameOver(FakeGame.DefaultGameId, out fakeQueueManager, reset);
            var controller = GetControllerForFakeGameWithQueueExpectingGameOver(manager);
            controller.DeleteConfirmed(FakeGame.DefaultGameId);
            reset.WaitOne(TimeSpan.FromSeconds(5));
            fakeQueueManager.VerifyAllExpectations();
        }

        [Test]
        public void PlayMoveChangesBoardCorrectly()
        {
            GameDto fakeGame = new FakeGame();

            var fakeRepo = MockRepository.GenerateMock<IGameRepository>();
            fakeRepo.Expect(x => x.FindById(FakeGame.DefaultGameId)).Return(fakeGame);
            var fakeHistoryRepo = MockRepository.GenerateMock<IHistoryRepository>();
            var fakeClockRepo = MockRepository.GenerateMock<IClockRepository>();

            var manager = new GameManager(fakeRepo, fakeHistoryRepo, fakeClockRepo);
            manager.Move(FakeGame.DefaultGameId, Location.E2, Location.E4);

            var args = fakeRepo.GetArgumentsForCallsMadeOn(a => a.AddOrUpdate(fakeGame));
            var historyArgs = fakeHistoryRepo.GetArgumentsForCallsMadeOn(a => a.Add(null));

            fakeRepo.VerifyAllExpectations();

            var updatedDto = args[0][0] as GameDto;
            var newHistoryEntry = historyArgs[0][0] as HistoryEntry;

            Assert.AreEqual("rnbqkbnr/pppppppp/8/8/4P3/8/PPPP1PPP/RNBQKBNR b KQkq E3 0", updatedDto.Fen, "Fen after move not as expected");
            Assert.AreEqual("rnbqkbnr/pppppppp/8/8/4P3/8/PPPP1PPP/RNBQKBNR b KQkq E3 0", newHistoryEntry.Fen, "Fen in history is wrong");
            Assert.AreEqual("e4", newHistoryEntry.Move, "Expected move to be e4");
            Assert.AreEqual(FakeGame.DefaultGameId, newHistoryEntry.GameId, "Expected history entry to refer to this game, " + FakeGame.DefaultGameId);
            Assert.AreEqual(1, newHistoryEntry.MoveNumber, "Expected this to be move 1");
        }

        [Test]
        public void MoveNumberIncrementsInHistory()
        {
            /* This method is full of hackery because movenumber is implemented in the database using a trigger, so it doesn't increment
             * from a mock. We need to make the mock do the increment. Also, as the board doesn't really change, the two moves are both 
             * by white. The point is to show that doing a move adds a history entry and AddOrUpdate is called the right number of times. */
            var fakeRepo = MockRepository.GenerateMock<IGameRepository>();
            fakeRepo.Expect(x => x.FindById(FakeGame.DefaultGameId)).WhenCalled(mi =>
            {
                GameDto retval = new FakeGame().WithMoveNumber(m_moveNumber);
                mi.ReturnValue = retval;
            });

            fakeRepo.Expect(x => x.AddOrUpdate(Arg<GameDto>.Is.Anything)).WhenCalled(mi =>
            {
                m_moveNumber++;
            });

            var fakeHistoryRepo = MockRepository.GenerateMock<IHistoryRepository>();
            var fakeClockRepo = MockRepository.GenerateMock<IClockRepository>();

            var manager = new GameManager(fakeRepo, fakeHistoryRepo, fakeClockRepo);
            manager.Move(FakeGame.DefaultGameId, Location.E2, Location.E4);
            manager.Move(FakeGame.DefaultGameId, Location.D2, Location.D3);

            var historyArgs = fakeHistoryRepo.GetArgumentsForCallsMadeOn(a => a.Add(Arg<HistoryEntry>.Is.Anything));
            fakeRepo.VerifyAllExpectations();
            var newHistoryEntryE4 = historyArgs[0][0] as HistoryEntry;
            var newHistoryEntryE5 = historyArgs[1][0] as HistoryEntry;

            Assert.AreEqual("e4", newHistoryEntryE4.Move, "Expected move to be e4");
            Assert.AreEqual(FakeGame.DefaultGameId, newHistoryEntryE4.GameId, "Expected history entry to refer to this game, " + FakeGame.DefaultGameId);
            Assert.AreEqual(1, newHistoryEntryE4.MoveNumber, "Expected this to be move 1");

            Assert.AreEqual("d3", newHistoryEntryE5.Move, "Expected move to be e5");
            Assert.AreEqual(FakeGame.DefaultGameId, newHistoryEntryE5.GameId, "Expected history entry to refer to this game, " + FakeGame.DefaultGameId);
            Assert.AreEqual(2, newHistoryEntryE5.MoveNumber, "Expected this to be move 2");
        }
    }
}
