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
            fakeGame.GameOver = true;
            return fakeGame;
        }

        [Test]
        public void PgnWriting()
        {
            var historyEntries = new[]
            {
                new HistoryEntry() { Fen = "rnbqkbnr/pppppppp/8/8/8/8/PPPPPPPP/RNBQKBNR w KQkq - 0", GameId = 10, HistoryId = 0, Move = "", MoveNumber = 1},
                new HistoryEntry() { Fen = "rnbqkbnr/pppp1ppp/8/413/8/8/PPPPPPPP/RNBQKBNR w KQkq - 0", GameId = 10, HistoryId = 1, Move = "e4", MoveNumber = 2}
            };

            var gameRepo = MockRepository.GenerateMock<IGameRepository>();
            gameRepo.Expect(x => x.FindById(10)).Return(GetFakeGame());

            var historyRepo = MockRepository.GenerateMock<IHistoryRepository>();
            historyRepo.Expect(x => x.FindAllMoves(10)).Return(historyEntries);

            var clockRepo = MockRepository.GenerateMock<IClockRepository>();

            var gameManager = new GameManager(gameRepo, historyRepo, clockRepo);
            var pgn = gameManager.PgnText(10);

            Assert.AreEqual(@"[Event ""Casual Game""]
[Site ""?""]
[Round ""?""]
[Date ""0001.01.01""]
[White ""clive""]
[Black ""james""]
[Result ""0-1""]

1. e4 0-1", pgn, "PGN of game not as expected");
        }
    }
}
