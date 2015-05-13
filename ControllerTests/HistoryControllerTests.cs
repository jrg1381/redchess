using System.Collections.Generic;
using System.Linq;
using System.Text;
using Chess.Controllers;
using NUnit.Framework;
using RedChess.WebEngine.Models;
using RedChess.WebEngine.Repositories;
using Rhino.Mocks;

namespace ControllerTests
{
    [TestFixture]
    public class HistoryControllerTests
    {
        private IGameBinding GetFakeGame()
        {
            var myUserProfile = new UserProfile { UserId = 23, UserName = "james" };
            var opponentUserProfile = new UserProfile { UserId = 27, UserName = "clive" };

            var fakeGame = MockRepository.GenerateStub<IGameBinding>();

            fakeGame.Expect(x => x.UserProfileBlack).Return(myUserProfile);
            fakeGame.Expect(x => x.UserProfileWhite).Return(opponentUserProfile);
            fakeGame.Expect(x => x.UserProfileWinner).Return(myUserProfile);
            fakeGame.Expect(x => x.GameOver).Return(true);
            return fakeGame;
        }

        [Test]
        public void PgnGeneration()
        {
            const string expectedPgn = @"[Event ""Casual Game""]
[Site ""?""]
[Round ""?""]
[Date ""0001.01.01""]
[White ""clive""]
[Black ""james""]
[Result ""0-1""]

1. e4 c6 2. d4 d5 3. e5 Bf5  0-1";

            var controller = new HistoryController();
            var fakeMoves = new List<HistoryEntry>();

            var moves = new[] {"", "e4","c6", "d4", "d5", "e5", "Bf5"};
            fakeMoves.AddRange(moves.Select(move => new HistoryEntry() {Move = move}));

            var fakeGame = GetFakeGame();

            var pgn = controller.GeneratePgn(fakeMoves, fakeGame);

            fakeGame.VerifyAllExpectations();
            Assert.AreEqual(expectedPgn, pgn, "PGN for short game not as expected");
        }

    }
}
