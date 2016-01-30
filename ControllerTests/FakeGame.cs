using Chess.Controllers;
using RedChess.WebEngine.Models;
using RedChess.WebEngine.Repositories;
using RedChess.WebEngine.Repositories.Interfaces;
using Rhino.Mocks;

namespace RedChess.ControllerTests
{
    /// <summary>
    /// Class representing a stub GameDto object. It presents a fluent interface for modifying the properties of the underlying object.
    /// </summary>
    class FakeGame
    {
        private readonly GameDto m_game;
        public static int DefaultGameId => 10;

        /// <summary>
        /// Allow the FakeGame to be used in place of the GameDto object by implicit cast
        /// </summary>
        /// <param name="b"></param>
        public static implicit operator GameDto(FakeGame b)
        {
            return b.m_game;
        }

        public FakeGame()
        {
            m_game = MockRepository.GenerateStub<GameDto>();
            var myUserProfile = new UserProfile { UserId = 23, UserName = "james" };
            var opponentUserProfile = new UserProfile { UserId = 27, UserName = "clive" };

            m_game.UserProfileBlack = myUserProfile;
            m_game.UserIdBlack = myUserProfile.UserId;
            m_game.UserProfileWhite = opponentUserProfile;
            m_game.UserIdWhite = opponentUserProfile.UserId;

            m_game.Fen = "rnbqkbnr/pppppppp/8/8/8/8/PPPPPPPP/RNBQKBNR w KQkq - 0";
            m_game.GameId = DefaultGameId;
            m_game.MoveNumber = 0;
        }

        public FakeGame WithId(int id)
        {
            m_game.GameId = id;
            return this;
        }

        public FakeGame WithFen(string fen)
        {
            m_game.Fen = fen;
            return this;
        }

        public FakeGame GameOver()
        {
            m_game.GameOver = true;
            return this;
        }

        public FakeGame WithMoveNumber(int moveNumber)
        {
            m_game.MoveNumber = moveNumber;
            return this;
        }

        public FakeGame WithStatus(string status)
        {
            m_game.Status = status;
            return this;
        }

        public FakeGame WithClaimableDraw(bool drawClaimable)
        {
            m_game.MayClaimDraw = drawClaimable;
            return this;
        }

        public FakeGame WithGameOver(bool gameOver)
        {
            m_game.GameOver = gameOver;
            return this;
        }

        public FakeGame BlackWins()
        {
            m_game.GameOver = true;
            m_game.UserIdWinner = m_game.UserIdBlack;
            m_game.UserProfileWinner = m_game.UserProfileBlack;
            return this;
        }

        public FakeGame WhiteWins()
        {
            m_game.GameOver = true;
            m_game.UserIdWinner = m_game.UserIdWhite;
            m_game.UserProfileWinner = m_game.UserProfileWhite;
            return this;
        }

        public static IGameRepository StubRepoForDefaultFakeGame()
        {
            var repository = MockRepository.GenerateStub<IGameRepository>();
            repository.Expect(x => x.FindById(DefaultGameId)).Return(new FakeGame());
            repository.Expect(x => x.FindById(Arg<int>.Is.NotEqual(DefaultGameId))).Return(null);
            return repository;
        }

        public static IGameRepository MockRepoForGame(GameDto fakeGame)
        {
            var repository = MockRepository.GenerateMock<IGameRepository>();
            repository.Expect(x => x.FindById(fakeGame.GameId)).Return(fakeGame);
            return repository;
        }

        public GameDto Build()
        {
            return (GameDto) this;
        }

        public static ICurrentUser StubIdentityProviderFor(string username, int userId = 33)
        {
            var identityProvider = MockRepository.GenerateStub<ICurrentUser>();
            identityProvider.Expect(x => x.CurrentUser).Return(username);
            identityProvider.Expect(x => x.CurrentUserId).Return(userId);
            return identityProvider;
        }
    }
}