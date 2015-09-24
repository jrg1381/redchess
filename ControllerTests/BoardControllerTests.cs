﻿using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Reflection;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;
using Chess.Filters;
using Newtonsoft.Json;
using NUnit.Framework;
using RedChess.WebEngine.Repositories;
using RedChess.ChessCommon.Enumerations;
using RedChess.WebEngine.Models;
using Chess.Controllers;
using RedChess.WebEngine.Repositories.Interfaces;
using Rhino.Mocks;

namespace ControllerTests
{
    [TestFixture]
    class BoardControllerTests
    {
        private const int c_fakeGameId = 10;

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
            var actionAllowedByFilter = PerformParticipantFiltering(controller, "DeleteMultiple");
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
            var controller = new BoardController(manager);
            bool ok = controller.MayManipulateBoard(c_fakeGameId, userName);
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
            var actionAllowedByFilter = PerformParticipantFiltering(controller, "DeleteConfirmed");

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
        /// <param name="actionName"></param>
        /// <returns></returns>
        private static bool PerformParticipantFiltering(BoardController controller, string actionName)
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

            var authFilter = new VerifyIsParticipantAttribute();
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
            var actionAllowedByFilter = PerformParticipantFiltering(controller, "Resign");
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
            var fakeGame = GetFakeGame();

            var fakeRepo = MockRepository.GenerateMock<IGameRepository>();
            fakeRepo.Expect(x => x.FindById(c_fakeGameId)).Return(fakeGame);
            var fakeHistoryRepo = MockRepository.GenerateMock<IHistoryRepository>();
            
            var fakeClockRepo = MockRepository.GenerateMock<IClockRepository>();

            var manager = new GameManager(fakeRepo, fakeHistoryRepo, fakeClockRepo);
            manager.Move(c_fakeGameId, Location.E2, Location.E4);
            manager.Move(c_fakeGameId, Location.E7, Location.E5);

            var historyArgs = fakeHistoryRepo.GetArgumentsForCallsMadeOn(a => a.Add(null));
            fakeRepo.VerifyAllExpectations();
            var newHistoryEntryE4 = historyArgs[0][0] as HistoryEntry;
            var newHistoryEntryE5 = historyArgs[1][0] as HistoryEntry;

            Assert.AreEqual("e4", newHistoryEntryE4.Move, "Expected move to be e4");
            Assert.AreEqual(c_fakeGameId, newHistoryEntryE4.GameId, "Expected history entry to refer to this game, " + c_fakeGameId);
            Assert.AreEqual(1, newHistoryEntryE4.MoveNumber, "Expected this to be move 1");

            Assert.AreEqual("e5", newHistoryEntryE5.Move, "Expected move to be e5");
            Assert.AreEqual(c_fakeGameId, newHistoryEntryE5.GameId, "Expected history entry to refer to this game, " + c_fakeGameId);
            Assert.AreEqual(2, newHistoryEntryE5.MoveNumber, "Expected this to be move 2");
        }
    }
}
