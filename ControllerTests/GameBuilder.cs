using System;
using RedChess.WebEngine.Models;
using RedChess.WebEngine.Repositories;
using RedChess.WebEngine.Repositories.Interfaces;
using Rhino.Mocks;

namespace RedChess.ControllerTests
{
    /// <summary>
    /// Class representing a stub GameDto object. It presents a fluent interface for modifying the properties of the underlying object.
    /// </summary>
    class GameBuilder
    {
        private readonly GameDto m_Game;
        public static int DefaultGameId => 10;

        /// <summary>
        /// Allow the GameBuilder to be used in place of the GameDto object by implicit cast
        /// </summary>
        /// <param name="builder"></param>
        public static implicit operator GameDto(GameBuilder builder)
        {
            return builder.m_Game;
        }

        public GameBuilder()
        {
            m_Game = MockRepository.GenerateStub<GameDto>();
            var myUserProfile = new UserProfile { UserId = 23, UserName = "james" };
            var opponentUserProfile = new UserProfile { UserId = 27, UserName = "clive" };

            m_Game.UserProfileBlack = myUserProfile;
            m_Game.UserIdBlack = myUserProfile.UserId;
            m_Game.UserProfileWhite = opponentUserProfile;
            m_Game.UserIdWhite = opponentUserProfile.UserId;

            m_Game.Fen = "rnbqkbnr/pppppppp/8/8/8/8/PPPPPPPP/RNBQKBNR w KQkq - 0";
            m_Game.GameId = DefaultGameId;
            m_Game.MoveNumber = 0;
        }

        public GameBuilder WithId(int id)
        {
            m_Game.GameId = id;
            return this;
        }

        public GameBuilder WithFen(string fen)
        {
            m_Game.Fen = fen;
            return this;
        }

        public GameBuilder GameOver()
        {
            m_Game.GameOver = true;
            return this;
        }

        public GameBuilder WithMoveNumber(int moveNumber)
        {
            m_Game.MoveNumber = moveNumber;
            return this;
        }

        public GameBuilder WithStatus(string status)
        {
            m_Game.Status = status;
            return this;
        }

        public GameBuilder WithClaimableDraw(bool drawClaimable)
        {
            m_Game.MayClaimDraw = drawClaimable;
            return this;
        }

        public GameBuilder WithGameOver(bool gameOver)
        {
            m_Game.GameOver = gameOver;
            return this;
        }

        public GameBuilder BlackWins()
        {
            m_Game.GameOver = true;
            m_Game.UserIdWinner = m_Game.UserIdBlack;
            m_Game.UserProfileWinner = m_Game.UserProfileBlack;
            return this;
        }

        public GameBuilder WhiteWins()
        {
            m_Game.GameOver = true;
            m_Game.UserIdWinner = m_Game.UserIdWhite;
            m_Game.UserProfileWinner = m_Game.UserProfileWhite;
            return this;
        }

        public static IGameRepository StubRepoForDefaultFakeGame()
        {
            var repository = MockRepository.GenerateStub<IGameRepository>();
            repository.Expect(x => x.FindById(DefaultGameId)).Return(new GameBuilder());
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

        internal GameBuilder WithTimeLimit(int timeLimitInSeconds)
        {
            m_Game.Clock = new Clock {TimeLimitMs = timeLimitInSeconds*1000};
            return this;
        }
    }
}