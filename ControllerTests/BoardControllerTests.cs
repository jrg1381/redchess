using System;
using System.Linq;
using NUnit.Framework;
using RedChess.WebEngine.Repositories;
using RedChess.ChessCommon.Enumerations;
using RedChess.WebEngine.Models;
using Chess.Controllers;
using Rhino.Mocks;

namespace ControllerTests
{
    [TestFixture]
    class BoardControllerTests
    {
        private GameDto GetFakeGame()
        {
            var myUserProfile = new UserProfile { UserId = 23, UserName = "james" };
            var opponentUserProfile = new UserProfile { UserId = 27, UserName = "clive" };

            var fakeGame = MockRepository.GenerateStub<GameDto>();
            fakeGame.UserProfileBlack = myUserProfile;
            fakeGame.UserProfileWhite = opponentUserProfile;
            fakeGame.Fen = "rnbqkbnr/pppppppp/8/8/8/8/PPPPPPPP/RNBQKBNR w KQkq - 0";
            fakeGame.GameId = 10;

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
            var fakeGame = GetFakeGame();

            var fakeRepo = MockRepository.GenerateMock<IGameRepository>();
            fakeRepo.Expect(x => x.FindById(10)).Return(fakeGame);
            var manager = new GameManager(fakeRepo);
            var controller = new BoardController(manager);
            bool ok = controller.MayManipulateBoard(10, userName);
            fakeRepo.VerifyAllExpectations();
            Assert.AreEqual(expectedResult, ok,"Permission to use board was not as expected");
        }

        private BoardController GetControllerForFakeGameAsUser(string userName, out IGameRepository repository)
        {
            var fakeGame = GetFakeGame();

            var fakeHistoryRepo = MockRepository.GenerateMock<IHistoryRepository>();
            var fakeClockRepo = MockRepository.GenerateMock<IClockRepository>();

            repository = MockRepository.GenerateMock<IGameRepository>();
            repository.Expect(x => x.FindById(10)).Return(fakeGame);
            var fakeIdentity = MockRepository.GenerateStub<ICurrentUser>();
            fakeIdentity.Stub(x => x.CurrentUser).Return(userName);

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

            if (allowed)
            {
                fakeRepo.Expect(x => x.Delete(10)).Repeat.Once();
            }
            else
            {
                fakeRepo.Expect(x => x.Delete(10)).Repeat.Never();
            }

            controller.DeleteConfirmed(10);
            fakeRepo.VerifyAllExpectations();
        }

        [TestCase("james", false)]
        [TestCase("clive", true)]
        [TestCase("jason", false)]
        public void CannotPlayOnOtherUsersGames(string userName, bool allowed)
        {
            IGameRepository fakeRepo;
            var controller = GetControllerForFakeGameAsUser(userName, out fakeRepo);
            var g = fakeRepo.FindById(10);

            if (allowed)
            {
                fakeRepo.Expect(x => x.AddOrUpdate(g)).Repeat.Once();
            }
            else
            {
                fakeRepo.Expect(x => x.AddOrUpdate(g)).Repeat.Never();
            }

            controller.PlayMove(10, "E2", "E4", "");
            fakeRepo.VerifyAllExpectations();
        }

        [TestCase("james", true)]
        [TestCase("clive", true)]
        [TestCase("jason", false)]
        public void CannotResignOtherUsersGames(string userName, bool allowed)
        {
            IGameRepository fakeRepo;
            var controller = GetControllerForFakeGameAsUser(userName, out fakeRepo);
            var g = fakeRepo.FindById(10);

            if (allowed)
            {
                fakeRepo.Expect(x => x.AddOrUpdate(g)).Repeat.Once();
            }
            else
            {
                fakeRepo.Expect(x => x.AddOrUpdate(g)).Repeat.Never();
            }

            controller.Resign(10);

            var args = fakeRepo.GetArgumentsForCallsMadeOn(a => a.AddOrUpdate(g));
            if (args.Count > 0)
            {
                var dto = args[0][0] as GameDto;
                Assert.AreEqual(allowed, dto.GameOver, "Expected game over state to change correctly");
            }

            fakeRepo.VerifyAllExpectations();
        }

        [Test]
        public void PlayMoveChangesBoardCorrectly()
        {
            var fakeGame = GetFakeGame();

            var fakeRepo = MockRepository.GenerateMock<IGameRepository>();
            fakeRepo.Expect(x => x.FindById(10)).Return(fakeGame);
            var fakeHistoryRepo = MockRepository.GenerateMock<IHistoryRepository>();
            var fakeClockRepo = MockRepository.GenerateMock<IClockRepository>();

            var manager = new GameManager(fakeRepo, fakeHistoryRepo, fakeClockRepo);
            manager.Move(10, Location.E2, Location.E4);

            var args = fakeRepo.GetArgumentsForCallsMadeOn(a => a.AddOrUpdate(fakeGame));
            var historyArgs = fakeHistoryRepo.GetArgumentsForCallsMadeOn(a => a.Add(null));

            fakeRepo.VerifyAllExpectations();

            var updatedDto = args[0][0] as GameDto;
            var newHistoryEntry = historyArgs[0][0] as HistoryEntry;

            Assert.AreEqual("rnbqkbnr/pppppppp/8/8/4P3/8/PPPP1PPP/RNBQKBNR b KQkq E3 0", updatedDto.Fen, "Fen after move not as expected");
            Assert.AreEqual("rnbqkbnr/pppppppp/8/8/4P3/8/PPPP1PPP/RNBQKBNR b KQkq E3 0", newHistoryEntry.Fen, "Fen in history is wrong");
            Assert.AreEqual("e4", newHistoryEntry.Move, "Expected move to be e4");
            Assert.AreEqual(10, newHistoryEntry.GameId, "Expected history entry to refer to this game, 10");
            Assert.AreEqual(1, newHistoryEntry.MoveNumber, "Expected this to be move 1");
        }

        [Test]
        public void MoveNumberIncrementsInHistory()
        {
            var fakeGame = GetFakeGame();
            int moveCount = 0;

            var fakeRepo = MockRepository.GenerateMock<IGameRepository>();
            fakeRepo.Expect(x => x.FindById(10)).Return(fakeGame);
            var fakeHistoryRepo = MockRepository.GenerateMock<IHistoryRepository>();
            fakeHistoryRepo.Expect(x => x.LatestMoveInGame(10)).WhenCalled(y => { y.ReturnValue = moveCount++; }).Return(0);
            
            var fakeClockRepo = MockRepository.GenerateMock<IClockRepository>();

            var manager = new GameManager(fakeRepo, fakeHistoryRepo, fakeClockRepo);
            manager.Move(10, Location.E2, Location.E4);
            manager.Move(10, Location.E7, Location.E5);

            var historyArgs = fakeHistoryRepo.GetArgumentsForCallsMadeOn(a => a.Add(null));
            fakeRepo.VerifyAllExpectations();
            var newHistoryEntryE4 = historyArgs[0][0] as HistoryEntry;
            var newHistoryEntryE5 = historyArgs[1][0] as HistoryEntry;

            Assert.AreEqual("e4", newHistoryEntryE4.Move, "Expected move to be e4");
            Assert.AreEqual(10, newHistoryEntryE4.GameId, "Expected history entry to refer to this game, 10");
            Assert.AreEqual(1, newHistoryEntryE4.MoveNumber, "Expected this to be move 1");

            Assert.AreEqual("e5", newHistoryEntryE5.Move, "Expected move to be e5");
            Assert.AreEqual(10, newHistoryEntryE5.GameId, "Expected history entry to refer to this game, 10");
            Assert.AreEqual(2, newHistoryEntryE5.MoveNumber, "Expected this to be move 2");
        }
    }
}
