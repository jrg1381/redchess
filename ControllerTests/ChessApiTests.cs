using System.Collections.Generic;
using Chess.Controllers;
using NUnit.Framework;
using RedChess.WebEngine.Models;
using RedChess.WebEngine.Repositories.Interfaces;
using Rhino.Mocks;
using System.Dynamic;
using System.Linq;
using System.Net;
using System.Web.Http.Results;
using System.Web.Mvc;
using RedChess.ChessCommon.Interfaces;
using RedChess.WebEngine.Repositories;

namespace RedChess.ControllerTests
{
    [TestFixture]
    public class ChessApiTests
    {
        private IEnumerable<TestCaseData> GamesForTestingMovesApi()
        {
            yield return new TestCaseData(
                @"{""Moves"":[{""Fen"":""rnbqkbnr/pppppppp/8/8/8/8/PPPPPPPP/RNBQKBNR w KQkq - 0"",""Move"":""""},{""Fen"":""rnbqkbnr/pppp1ppp/8/4p3/8/8/PPPPPPPP/RNBQKBNR w KQkq - 0"",""Move"":""e4""}],""Description"":""clive vs james"",""IsParticipant"":true,""Analysis"":null,""GameOver"":false,""Status"":null,""Winner"":""""}",
                new GameBuilder().Build()
                );

            yield return new TestCaseData(
                @"{""Moves"":[{""Fen"":""rnbqkbnr/pppppppp/8/8/8/8/PPPPPPPP/RNBQKBNR w KQkq - 0"",""Move"":""""},{""Fen"":""rnbqkbnr/pppp1ppp/8/4p3/8/8/PPPPPPPP/RNBQKBNR w KQkq - 0"",""Move"":""e4""}],""Description"":""clive vs james"",""IsParticipant"":true,""Analysis"":null,""GameOver"":true,""Status"":null,""Winner"":""""}",
                new GameBuilder().GameOver().Build()
                );
            yield return new TestCaseData(
                @"{""Moves"":[{""Fen"":""rnbqkbnr/pppppppp/8/8/8/8/PPPPPPPP/RNBQKBNR w KQkq - 0"",""Move"":""""},{""Fen"":""rnbqkbnr/pppp1ppp/8/4p3/8/8/PPPPPPPP/RNBQKBNR w KQkq - 0"",""Move"":""e4""}],""Description"":""clive vs james"",""IsParticipant"":true,""Analysis"":null,""GameOver"":true,""Status"":null,""Winner"":""w""}",
                new GameBuilder().WhiteWins().Build()
                );
            yield return new TestCaseData(
                @"{""Moves"":[{""Fen"":""rnbqkbnr/pppppppp/8/8/8/8/PPPPPPPP/RNBQKBNR w KQkq - 0"",""Move"":""""},{""Fen"":""rnbqkbnr/pppp1ppp/8/4p3/8/8/PPPPPPPP/RNBQKBNR w KQkq - 0"",""Move"":""e4""}],""Description"":""clive vs james"",""IsParticipant"":true,""Analysis"":null,""GameOver"":true,""Status"":null,""Winner"":""b""}",
                new GameBuilder().BlackWins().Build()
                );
        }

        [TestCaseSource(nameof(GamesForTestingMovesApi))]
        public void MovesReturnsMoves(string expectedJson, GameDto gameDto)
        {
            var historyEntries = GetHistoryEntries();

            var mockManager = MockRepository.GenerateMock<IGameManager>();
            var gameBinding = new GameBinding(gameDto, mockManager); 

            mockManager.Expect(x => x.FetchGame(gameDto.GameId)).Return(gameBinding);
            mockManager.Expect(x => x.FindAllMoves(Arg<int>.Is.Equal(gameDto.GameId))).Return(historyEntries);
            var controller = new ChessApiController(mockManager, IdentityProviders.StubIdentityProviderFor(gameDto.UserProfileWhite.UserName, gameDto.UserProfileWhite.UserId));
            var result = controller.Moves(gameDto.GameId) as JsonResult<ExpandoObject>;
            dynamic d = result.Content;
            mockManager.VerifyAllExpectations();
            var json = Newtonsoft.Json.JsonConvert.SerializeObject(d);
            Assert.AreEqual(expectedJson, json, "expected JSON for getting moves not correct");
        }

        [Test]
        public void MovesReturns404WhenNoGameOrNoMoves()
        {
            var mockManager = MockRepository.GenerateMock<IGameManager>();

            mockManager.Expect(x => x.FetchGame(Arg<int>.Is.Anything)).Return(null);
            mockManager.Expect(x => x.FindAllMoves(Arg<int>.Is.Anything)).Return(Enumerable.Empty<HistoryEntry>());
            var controller = new ChessApiController(mockManager, IdentityProviders.StubIdentityProviderFor("james"));
            var result = controller.Moves(1234) as StatusCodeResult;
            mockManager.VerifyAllExpectations();
            Assert.AreEqual(HttpStatusCode.NotFound, result.StatusCode, "Expected 404 when game does not exist");
        }

        private static HistoryEntry[] GetHistoryEntries()
        {
            var historyEntries = new[]
            {
                new HistoryEntry()
                {
                    Fen = "rnbqkbnr/pppppppp/8/8/8/8/PPPPPPPP/RNBQKBNR w KQkq - 0",
                    GameId = GameBuilder.DefaultGameId,
                    HistoryId = 0,
                    Move = "",
                    MoveNumber = 1
                },
                new HistoryEntry()
                {
                    Fen = "rnbqkbnr/pppp1ppp/8/4p3/8/8/PPPPPPPP/RNBQKBNR w KQkq - 0",
                    GameId = GameBuilder.DefaultGameId,
                    HistoryId = 1,
                    Move = "e4",
                    MoveNumber = 2
                }
            };
            return historyEntries;
        }
    }
}
