using System;
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
            var fakeGame = MockRepository.GenerateMock<GameDto>();
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
            var fakeRepo = MockRepository.GenerateMock<IGameRepository>();
            fakeRepo.Expect(x => x.Delete(40));
            var manager = new GameManager(fakeRepo);
            var controller = new BoardController(manager);
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
            var manager = new GameManager(fakeRepo);
            var controller = new BoardController(manager);
            controller.DeleteMultiple("10,20,30,40");

            fakeRepo.VerifyAllExpectations();
        }

        [Test]
        public void DeleteMultipleOneArgumentCallsDelete()
        {
            var fakeRepo = MockRepository.GenerateStrictMock<IGameRepository>();
            fakeRepo.Expect(x => x.Delete(10));
            var manager = new GameManager(fakeRepo);
            var controller = new BoardController(manager);
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

            var fakeGame = MockRepository.GenerateStub<GameDto>();
            fakeGame.UserProfileBlack = myUserProfile;
            fakeGame.UserProfileWhite = opponentUserProfile;

            var fakeRepo = MockRepository.GenerateMock<IGameRepository>();
            fakeRepo.Expect(x => x.FindById(10)).Return(fakeGame);
            var manager = new GameManager(fakeRepo);
            var controller = new BoardController(manager);
            bool ok = controller.MayManipulateBoard(10, userName);
            fakeRepo.VerifyAllExpectations();
            Assert.AreEqual(expectedResult, ok,"Permission to use board was not as expected");
        }

        [Test]
        public void PlayMoveChangesBoardCorrectly()
        {
            var myUserProfile = new UserProfile { UserId = 23, UserName = "james" };
            var opponentUserProfile = new UserProfile { UserId = 27, UserName = "clive" };

            var fakeGame = MockRepository.GenerateMock<GameDto>();
            fakeGame.UserProfileBlack = myUserProfile;
            fakeGame.UserProfileWhite = opponentUserProfile;
            fakeGame.Fen = "rnbqkbnr/pppppppp/8/8/8/8/PPPPPPPP/RNBQKBNR w KQkq - 0";
            fakeGame.GameId = 10;

            var identityProvider = MockRepository.GenerateStub<ICurrentUser>();
            identityProvider.Stub(x => x.CurrentUser).Return("james");

            var fakeRepo = MockRepository.GenerateMock<IGameRepository>();
            fakeRepo.Expect(x => x.FindById(10)).Return(fakeGame);
            var fakeHistoryRepo = MockRepository.GenerateMock<IHistoryRepository>();
            var fakeClockRepo = MockRepository.GenerateMock<IClockRepository>();

            var manager = new GameManager(fakeRepo, fakeHistoryRepo, fakeClockRepo);
            manager.Move(10, Location.E2, Location.E4);

            var args = fakeRepo.GetArgumentsForCallsMadeOn(a => a.AddOrUpdate(fakeGame));
            fakeRepo.VerifyAllExpectations();
            var f = args[0][0] as GameDto;
            Assert.AreEqual("rnbqkbnr/pppppppp/8/8/4P3/8/PPPP1PPP/RNBQKBNR b KQkq E3 0", f.Fen, "Fen after move not as expected");
        }
    }
}
