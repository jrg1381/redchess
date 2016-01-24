using RedChess.WebEngine.Models;
using RedChess.WebEngine.Repositories;
using Rhino.Mocks;

namespace RedChess.ControllerTests
{
    /// <summary>
    /// Class representing a stub GameDto object. It presents a fluent interface for modifying the properties of the underlying object.
    /// </summary>
    class FakeGame
    {
        const int c_fakeGameId = 10;
        private readonly GameDto m_game;

        public static int DefaultGameId => c_fakeGameId;

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
            m_game.GameId = c_fakeGameId;
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
    }
}