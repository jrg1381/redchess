using System.Collections.Specialized;
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
        private const int c_fakeGameId = 10;
        private int m_moveNumber = 0;

        private GameDto GetFakeGame(int id = c_fakeGameId)
        {
            var myUserProfile = new UserProfile { UserId = 23, UserName = "james" };
            var opponentUserProfile = new UserProfile { UserId = 27, UserName = "clive" };

            var fakeGame = MockRepository.GenerateStub<GameDto>();

            fakeGame.UserProfileBlack = myUserProfile;
            fakeGame.UserIdBlack = myUserProfile.UserId; 
            fakeGame.UserProfileWhite = opponentUserProfile;
            fakeGame.UserIdWhite = opponentUserProfile.UserId;

            fakeGame.Fen = "rnbqkbnr/pppppppp/8/8/8/8/PPPPPPPP/RNBQKBNR w KQkq - 0";
            fakeGame.GameId = id;
            fakeGame.MoveNumber = 0;

            return fakeGame;
        }

        private GameDto GetFakeGamePromotionImminent(int id = c_fakeGameId)
        {
            var myUserProfile = new UserProfile { UserId = 23, UserName = "james" };
            var opponentUserProfile = new UserProfile { UserId = 27, UserName = "clive" };

            var fakeGame = MockRepository.GenerateStub<GameDto>();

            fakeGame.UserProfileBlack = myUserProfile;
            fakeGame.UserIdBlack = myUserProfile.UserId;
            fakeGame.UserProfileWhite = opponentUserProfile;
            fakeGame.UserIdWhite = opponentUserProfile.UserId;

            fakeGame.Fen = "8/P7/8/8/8/8/8/K6k w - - 0";
            fakeGame.GameId = id;
            fakeGame.MoveNumber = 0;

            return fakeGame;
        }

        private BoardController GetControllerForFakeEndedGameAsUser(string userName, out IGameRepository repository)
        {
            var fakeGame = GetFakeGame();
            fakeGame.GameOver = true;

            var fakeHistoryRepo = MockRepository.GenerateMock<IHistoryRepository>();
            var fakeClockRepo = MockRepository.GenerateMock<IClockRepository>();

            repository = MockRepository.GenerateMock<IGameRepository>();
            repository.Expect(x => x.FindById(c_fakeGameId)).Return(fakeGame);
            var fakeIdentity = MockRepository.GenerateStub<ICurrentUser>();
            fakeIdentity.Stub(x => x.CurrentUser).Return(userName);

            var manager = new GameManager(repository, fakeHistoryRepo, fakeClockRepo);
            return new BoardController(manager, fakeIdentity);
        }

        private BoardController GetControllerForFakeGameWithQueue(out IQueueManager fakeQueueManager, bool expectPostMessage)
        {
            var fakeGame = GetFakeGame();

            var fakeHistoryRepo = MockRepository.GenerateMock<IHistoryRepository>();
            var fakeClockRepo = MockRepository.GenerateMock<IClockRepository>();

            var repository = MockRepository.GenerateMock<IGameRepository>();
            repository.Expect(x => x.FindById(c_fakeGameId)).Return(fakeGame);
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

        private BoardController GetControllerForFakeGameWithQueueWithPromotionImminent(out IQueueManager fakeQueueManager)
        {
            var fakeGame = GetFakeGamePromotionImminent();

            var fakeHistoryRepo = MockRepository.GenerateMock<IHistoryRepository>();
            var fakeClockRepo = MockRepository.GenerateMock<IClockRepository>();

            var repository = MockRepository.GenerateMock<IGameRepository>();
            repository.Expect(x => x.FindById(c_fakeGameId)).Return(fakeGame);
            var fakeIdentity = MockRepository.GenerateStub<ICurrentUser>();
            fakeIdentity.Stub(x => x.CurrentUser).Return("clive");
            fakeQueueManager = MockRepository.GenerateMock<IQueueManager>();
            fakeQueueManager.Expect(x => x.PostRequestBestMoveMessage(10, 0, fakeGame.Fen, "a7a8q")).Repeat.Once();

            var manager = new GameManager(repository, fakeHistoryRepo, fakeClockRepo, fakeQueueManager);
            return new BoardController(manager, fakeIdentity);
        }

        private GameDto GetFakeGameAboutToPromote()
        {
            var myUserProfile = new UserProfile { UserId = 23, UserName = "james" };
            var opponentUserProfile = new UserProfile { UserId = 27, UserName = "clive" };

            var fakeGame = MockRepository.GenerateStub<GameDto>();
            fakeGame.UserProfileBlack = myUserProfile;
            fakeGame.UserProfileWhite = opponentUserProfile;
            fakeGame.Fen = "k6K/8/8/8/8/8/7p/8 b - - 0";
            fakeGame.GameId = c_fakeGameId;

            return fakeGame;
        }

        [Test]
        public void GetDetailsCallsFindById()
        {
            var fakeGame = GetFakeGame();
            var fakeRepo = MockRepository.GenerateMock<IGameRepository>();
            fakeRepo.Expect(x => x.FindById(40)).Return(fakeGame);
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

            var manager = new GameManager(fakeRepo);
            var controller = new BoardController(manager, fakeIdProvider);
            controller.DeleteConfirmed(40);

            fakeRepo.VerifyAllExpectations();
        }

        [Test]
        public void DeleteMultipleCallsDelete()
        {
            var fakeRepo = MockRepository.GenerateStrictMock<IGameRepository>();

            fakeRepo.Expect(x => x.Delete(c_fakeGameId));
            fakeRepo.Expect(x => x.Delete(20));
            fakeRepo.Expect(x => x.Delete(30));
            fakeRepo.Expect(x => x.Delete(40));
            var manager = new GameManager(fakeRepo);
            var fakeIdentity = MockRepository.GenerateStub<ICurrentUser>();
            fakeIdentity.Stub(x => x.CurrentUser).Return("james");
            var controller = new BoardController(manager, fakeIdentity);
            controller.DeleteMultiple(c_fakeGameId + ",20,30,40");

            fakeRepo.VerifyAllExpectations();
        }

        [Test]
        public void DeleteMultipleCannotDeleteArbitraryGames()
        {
            var fakeRepo = MockRepository.GenerateStrictMock<IGameRepository>();
            fakeRepo.Expect(x => x.FindById(c_fakeGameId)).Return(GetFakeGame());
            fakeRepo.AssertWasNotCalled(x => x.Delete(c_fakeGameId));
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
            fakeRepo.Expect(x => x.Delete(c_fakeGameId));
            var manager = new GameManager(fakeRepo);
            var fakeIdentity = MockRepository.GenerateStub<ICurrentUser>();
            fakeIdentity.Stub(x => x.CurrentUser).Return("james");
            var controller = new BoardController(manager, fakeIdentity);
            controller.DeleteMultiple(c_fakeGameId.ToString());

            fakeRepo.VerifyAllExpectations();
        }

        [TestCase("james", true)]
        [TestCase("clive", true)]
        [TestCase("jason", false)]
        public void MayManipulateBoardTest(string userName, bool expectedResult)
        {
            var fakeGame = GetFakeGame();

            var fakeRepo = MockRepository.GenerateMock<IGameRepository>();
            fakeRepo.Expect(x => x.FindById(c_fakeGameId)).Return(fakeGame);
            var manager = new GameManager(fakeRepo);
            var identityProvider = MockRepository.GenerateMock<ICurrentUser>();
            identityProvider.Expect(x => x.CurrentUser).Return(userName);
            var accessValidator = new AccessValidator(manager, identityProvider);
            bool ok = accessValidator.MayAccess(c_fakeGameId);
            fakeRepo.VerifyAllExpectations();
            Assert.AreEqual(expectedResult, ok,"Permission to use board was not as expected");
        }

        private BoardController GetControllerForFakeGameAsUser(string userName, out IGameRepository repository)
        {
            var fakeGame = GetFakeGame();

            var fakeHistoryRepo = MockRepository.GenerateMock<IHistoryRepository>();
            var fakeClockRepo = MockRepository.GenerateMock<IClockRepository>();

            repository = MockRepository.GenerateMock<IGameRepository>();
            repository.Expect(x => x.FindById(c_fakeGameId)).Return(fakeGame);
            var fakeIdentity = MockRepository.GenerateStub<ICurrentUser>();
            fakeIdentity.Stub(x => x.CurrentUser).Return(userName);

            var manager = new GameManager(repository, fakeHistoryRepo, fakeClockRepo);
            return new BoardController(manager, fakeIdentity);
        }

        private BoardController GetControllerForFakeGameAsUserWithClock(string userName, out IGameRepository repository)
        {
            var fakeGame = GetFakeGame();

            var fakeHistoryRepo = MockRepository.GenerateMock<IHistoryRepository>();
            var fakeClockRepo = MockRepository.GenerateMock<IClockRepository>();

            repository = MockRepository.GenerateMock<IGameRepository>();
            repository.Expect(x => x.FindById(c_fakeGameId)).Return(fakeGame);
            var fakeIdentity = MockRepository.GenerateStub<ICurrentUser>();
            fakeIdentity.Stub(x => x.CurrentUser).Return(userName);
            fakeClockRepo.Expect(x => x.Clock(c_fakeGameId)).Return(new Clock());

            var manager = new GameManager(repository, fakeHistoryRepo, fakeClockRepo);
            return new BoardController(manager, fakeIdentity);
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
                fakeRepo.Expect(x => x.Delete(c_fakeGameId)).Repeat.Once();
                controller.DeleteConfirmed(c_fakeGameId);
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
        /// <param name="currentUser"></param>
        /// <param name="actionName"></param>
        /// <returns></returns>
        private static bool PerformParticipantFiltering(Controller controller, IGameManager manager, ICurrentUser currentUser, string actionName)
        {
            var httpContext = MockRepository.GenerateMock<HttpContextBase>();
            var httpRequest = MockRepository.GenerateMock<HttpRequestBase>();
            var theParams = new NameValueCollection();
            theParams["id"] = c_fakeGameId.ToString();
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
            var g = fakeRepo.FindById(c_fakeGameId);

            if (allowed)
            {
                fakeRepo.Expect(x => x.AddOrUpdate(g)).Repeat.Once();
            }
            else
            {
                fakeRepo.Expect(x => x.AddOrUpdate(g)).Repeat.Never();
            }

            controller.PlayMove(c_fakeGameId, "E2", "E4", "");
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
            var manager = new GameManager(fakeRepo);

            var actionAllowedByFilter = PerformParticipantFiltering(controller, manager, fakeIdentity, "Resign");
            var g = fakeRepo.FindById(c_fakeGameId);

            if (allowed)
            {
                fakeRepo.Expect(x => x.AddOrUpdate(g)).Repeat.Once();
                Assert.IsTrue(actionAllowedByFilter, "Filter should not have denied us");
            }
            else
            {
                Assert.IsFalse(actionAllowedByFilter, "Filter should have denied us");
            }

            controller.Resign(c_fakeGameId);

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
            var result = controller.PlayMove(c_fakeGameId, "E2", "E4", "");
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
            var g = fakeRepo.FindById(c_fakeGameId);

            controller.TimedOut(c_fakeGameId, message, timedOutColor);

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
        public void PlayMoveChangesBoardCorrectlyForPromotion()
        {
            var fakeGame = GetFakeGameAboutToPromote();

            var fakeRepo = MockRepository.GenerateMock<IGameRepository>();
            fakeRepo.Expect(x => x.FindById(c_fakeGameId)).Return(fakeGame);
            var fakeHistoryRepo = MockRepository.GenerateMock<IHistoryRepository>();
            var fakeClockRepo = MockRepository.GenerateMock<IClockRepository>();

            var manager = new GameManager(fakeRepo, fakeHistoryRepo, fakeClockRepo);
            manager.Move(c_fakeGameId, Location.H2, Location.H1, "Q");

            var args = fakeRepo.GetArgumentsForCallsMadeOn(a => a.AddOrUpdate(fakeGame));

            fakeRepo.VerifyAllExpectations();

            var updatedDto = args[0][0] as GameDto;

            Assert.AreEqual("k6K/8/8/8/8/8/8/7q w - - 0", updatedDto.Fen, "Fen after move not as expected");
        }

        [Test]
        public void PlayMoveChangesBoardCorrectly()
        {
            var fakeGame = GetFakeGame();

            var fakeRepo = MockRepository.GenerateMock<IGameRepository>();
            fakeRepo.Expect(x => x.FindById(c_fakeGameId)).Return(fakeGame);
            var fakeHistoryRepo = MockRepository.GenerateMock<IHistoryRepository>();
            var fakeClockRepo = MockRepository.GenerateMock<IClockRepository>();

            var manager = new GameManager(fakeRepo, fakeHistoryRepo, fakeClockRepo);
            manager.Move(c_fakeGameId, Location.E2, Location.E4);

            var args = fakeRepo.GetArgumentsForCallsMadeOn(a => a.AddOrUpdate(fakeGame));
            var historyArgs = fakeHistoryRepo.GetArgumentsForCallsMadeOn(a => a.Add(null));

            fakeRepo.VerifyAllExpectations();

            var updatedDto = args[0][0] as GameDto;
            var newHistoryEntry = historyArgs[0][0] as HistoryEntry;

            Assert.AreEqual("rnbqkbnr/pppppppp/8/8/4P3/8/PPPP1PPP/RNBQKBNR b KQkq E3 0", updatedDto.Fen, "Fen after move not as expected");
            Assert.AreEqual("rnbqkbnr/pppppppp/8/8/4P3/8/PPPP1PPP/RNBQKBNR b KQkq E3 0", newHistoryEntry.Fen, "Fen in history is wrong");
            Assert.AreEqual("e4", newHistoryEntry.Move, "Expected move to be e4");
            Assert.AreEqual(c_fakeGameId, newHistoryEntry.GameId, "Expected history entry to refer to this game, " + c_fakeGameId);
            Assert.AreEqual(1, newHistoryEntry.MoveNumber, "Expected this to be move 1");
        }

        [Test]
        public void MoveNumberIncrementsInHistory()
        {
            /* This method is full of hackery because movenumber is implemented in the database using a trigger, so it doesn't increment
             * from a mock. We need to make the mock do the increment. Also, as the board doesn't really change, the two moves are both 
             * by white. The point is to show that doing a move adds a history entry and AddOrUpdate is called the right number of times. */
            var fakeRepo = MockRepository.GenerateMock<IGameRepository>();
            fakeRepo.Expect(x => x.FindById(c_fakeGameId)).WhenCalled(mi =>
            {
                var retval = GetFakeGame(c_fakeGameId);
                retval.MoveNumber = m_moveNumber;
                mi.ReturnValue = retval;
            });

            fakeRepo.Expect(x => x.AddOrUpdate(Arg<GameDto>.Is.Anything)).WhenCalled(mi =>
            {
                m_moveNumber++;
            });

            var fakeHistoryRepo = MockRepository.GenerateMock<IHistoryRepository>();
            var fakeClockRepo = MockRepository.GenerateMock<IClockRepository>();

            var manager = new GameManager(fakeRepo, fakeHistoryRepo, fakeClockRepo);
            manager.Move(c_fakeGameId, Location.E2, Location.E4);
            manager.Move(c_fakeGameId, Location.D2, Location.D3);

            var historyArgs = fakeHistoryRepo.GetArgumentsForCallsMadeOn(a => a.Add(Arg<HistoryEntry>.Is.Anything));
            fakeRepo.VerifyAllExpectations();
            var newHistoryEntryE4 = historyArgs[0][0] as HistoryEntry;
            var newHistoryEntryE5 = historyArgs[1][0] as HistoryEntry;

            Assert.AreEqual("e4", newHistoryEntryE4.Move, "Expected move to be e4");
            Assert.AreEqual(c_fakeGameId, newHistoryEntryE4.GameId, "Expected history entry to refer to this game, " + c_fakeGameId);
            Assert.AreEqual(1, newHistoryEntryE4.MoveNumber, "Expected this to be move 1");

            Assert.AreEqual("d3", newHistoryEntryE5.Move, "Expected move to be e5");
            Assert.AreEqual(c_fakeGameId, newHistoryEntryE5.GameId, "Expected history entry to refer to this game, " + c_fakeGameId);
            Assert.AreEqual(2, newHistoryEntryE5.MoveNumber, "Expected this to be move 2");
        }
    }
}
