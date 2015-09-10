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
    }
}
