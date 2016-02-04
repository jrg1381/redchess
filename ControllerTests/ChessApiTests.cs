using Chess.Controllers;
using NUnit.Framework;
using RedChess.WebEngine.Models;
using RedChess.WebEngine.Repositories.Interfaces;
using Rhino.Mocks;
using System.Dynamic;
using System.Web.Http.Results;
using RedChess.WebEngine.Repositories;

namespace RedChess.ControllerTests
{
    [TestFixture]
    public class ChessApiTests
    {
        [Test]
        public void MovesReturnsMoves()
        {
            var historyEntries = GetHistoryEntries();

            var expectedJson =
                @"{""Moves"":[{""Fen"":""rnbqkbnr/pppppppp/8/8/8/8/PPPPPPPP/RNBQKBNR w KQkq - 0"",""Move"":""""},{""Fen"":""rnbqkbnr/pppp1ppp/8/4p3/8/8/PPPPPPPP/RNBQKBNR w KQkq - 0"",""Move"":""e4""}],""Description"":""clive vs james"",""IsParticipant"":true,""Analysis"":null,""GameOver"":false,""Status"":null,""Winner"":""""}";

            var fakeGame = new FakeGame().Build();
            var mockManager = MockRepository.GenerateMock<IGameManager>();
            var gameBinding = new GameBinding(fakeGame, mockManager);

            mockManager.Expect(x => x.FetchGame(FakeGame.DefaultGameId)).Return(gameBinding);
            mockManager.Expect(x => x.FindAllMoves(Arg<int>.Is.Equal(FakeGame.DefaultGameId))).Return(historyEntries);
            var controller = new ChessApiController(mockManager, FakeGame.StubIdentityProviderFor(fakeGame.UserProfileWhite.UserName, fakeGame.UserProfileWhite.UserId));
            var result = controller.Moves(FakeGame.DefaultGameId) as JsonResult<ExpandoObject>;
            dynamic d = result.Content;
            mockManager.VerifyAllExpectations();
            var json = Newtonsoft.Json.JsonConvert.SerializeObject(d);
            Assert.AreEqual(expectedJson, json, "expected JSON for getting moves not correct");
        }

        private static HistoryEntry[] GetHistoryEntries()
        {
            var historyEntries = new[]
            {
                new HistoryEntry()
                {
                    Fen = "rnbqkbnr/pppppppp/8/8/8/8/PPPPPPPP/RNBQKBNR w KQkq - 0",
                    GameId = FakeGame.DefaultGameId,
                    HistoryId = 0,
                    Move = "",
                    MoveNumber = 1
                },
                new HistoryEntry()
                {
                    Fen = "rnbqkbnr/pppp1ppp/8/4p3/8/8/PPPPPPPP/RNBQKBNR w KQkq - 0",
                    GameId = FakeGame.DefaultGameId,
                    HistoryId = 1,
                    Move = "e4",
                    MoveNumber = 2
                }
            };
            return historyEntries;
        }
    }
}
