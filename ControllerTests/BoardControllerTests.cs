﻿using System;
using NUnit.Framework;
using Chess.Controllers;
using Chess.Models;
using Chess.Repositories;
using RedChess.ChessCommon.Enumerations;
using Rhino.Mocks;

namespace ControllerTests
{
    [TestFixture]
    class BoardControllerTests
    {
        [Test]
        public void GetDetailsCallsFindById()
        {
            var fakeGame = MockRepository.GenerateMock<Game>();
            var fakeRepo = MockRepository.GenerateMock<IGameRepository>();
            fakeRepo.Expect(x => x.FindById(40)).Return(fakeGame);
            fakeRepo.Replay();

            var controller = new BoardController(fakeRepo);
            controller.Details(40);
           
            fakeRepo.VerifyAllExpectations();
        }

        [Test]
        public void DeleteConfirmedCallsDelete()
        {
            var fakeRepo = MockRepository.GenerateMock<IGameRepository>();
            fakeRepo.Expect(x => x.Delete(40));

            var controller = new BoardController(fakeRepo);
            controller.DeleteConfirmed(40);

            fakeRepo.VerifyAllExpectations();
        }

        [Test]
        public void DeleteMultipleCallsDelete()
        {
            var fakeRepo = MockRepository.GenerateStrictMock<IGameRepository>();
            fakeRepo.Expect(x => x.Delete(10));
            fakeRepo.Expect(x => x.Delete(20));
            fakeRepo.Expect(x => x.Delete(30));
            fakeRepo.Expect(x => x.Delete(40));

            var controller = new BoardController(fakeRepo);
            controller.DeleteMultiple("10,20,30,40");

            fakeRepo.VerifyAllExpectations();
        }

        [Test]
        public void DeleteMultipleOneArgumentCallsDelete()
        {
            var fakeRepo = MockRepository.GenerateStrictMock<IGameRepository>();
            fakeRepo.Expect(x => x.Delete(10));

            var controller = new BoardController(fakeRepo);
            controller.DeleteMultiple("10");

            fakeRepo.VerifyAllExpectations();
        }

        [TestCase("james", true)]
        [TestCase("clive", true)]
        [TestCase("jason", false)]
        public void MayManipulateBoardTest(string userName, bool expectedResult)
        {
            var myUserProfile = new UserProfile {UserId = 23, UserName = "james"};
            var opponentUserProfile = new UserProfile { UserId = 27, UserName = "clive" };

            var fakeGame = MockRepository.GenerateStub<IGame>();
            fakeGame.Stub(x => x.UserProfileBlack).Return(myUserProfile);
            fakeGame.Stub(x => x.UserProfileWhite).Return(opponentUserProfile);

            var fakeRepo = MockRepository.GenerateMock<IGameRepository>();
            fakeRepo.Expect(x => x.FindById(10)).Return(fakeGame);

            var controller = new BoardController(fakeRepo);
            bool ok = controller.MayManipulateBoard(10, userName);
            fakeRepo.VerifyAllExpectations();
            Assert.AreEqual(expectedResult, ok,"Permission to use board was not as expected");
        }

        [Test]
        public void PlayMoveChangesBoardCorrectly()
        {
            var myUserProfile = new UserProfile { UserId = 23, UserName = "james" };
            var opponentUserProfile = new UserProfile { UserId = 27, UserName = "clive" };

            var fakeGame = MockRepository.GenerateMock<IGame>();
            fakeGame.Stub(x => x.UserProfileBlack).Return(myUserProfile);
            fakeGame.Stub(x => x.UserProfileWhite).Return(opponentUserProfile);
            fakeGame.Stub(x => x.IsUsersTurn("james")).Return(true);
            fakeGame.Stub(x => x.Id).Return(10);
            fakeGame.Expect(x => x.Move(Location.E2, Location.E4)).Return(true);

            var identityProvider = MockRepository.GenerateStub<ICurrentUser>();
            identityProvider.Stub(x => x.CurrentUser).Return("james");

            var fakeRepo = MockRepository.GenerateMock<IGameRepository>();
            fakeRepo.Expect(x => x.FindById(10)).Return(fakeGame);

            var controller = new BoardController(fakeRepo, identityProvider);
            controller.PlayMove(10, "E2", "E4", "");

            Assert.AreEqual("", fakeGame.Fen, "fek");
        }
    }
}
