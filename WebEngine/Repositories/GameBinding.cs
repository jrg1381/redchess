using System;
using RedChess.WebEngine.Models;
using RedChess.WebEngine.Repositories.Interfaces;

namespace RedChess.WebEngine.Repositories
{
    /// <summary>
    /// A GameBinding is a wrapper around a GameDto, which is the .NET representation of the Boards table
    /// in the database. The GameBinding extends the GameDto with some utility properties which make it more
    /// suitable for the role of the model in an MVC view.
    /// </summary>
    internal class GameBinding : IGameBinding
    {
        private readonly GameDto m_gameDto;
        private readonly GameManager m_gameManager;

        public GameBinding(GameDto gameDto, GameManager gameManager)
        {
            m_gameDto = gameDto;
            m_gameManager = gameManager;
        }

        /// <summary>
        /// A description of the game in "Foo vs Bar" format.
        /// </summary>
        public string Description
        {
            get
            {
                return String.Format("{0} vs {1}", m_gameDto.UserProfileWhite.UserName, m_gameDto.UserProfileBlack.UserName);
            }
        }

        /// <summary>
        /// Returns true if the game has not yet started (both players have not sent a start request)
        /// </summary>
        public bool ShouldLockUi
        {
            get { return m_gameManager.ShouldLockUi(m_gameDto.GameId); }
        }

        /// <summary>
        /// A string describing the state of the game : game over, check, checkmate, resigned...
        /// </summary>
        public string Status
        {
            get { return m_gameDto.Status; }
        }

        /// <summary>
        /// Game ID
        /// </summary>
        public int GameId
        {
            get { return m_gameDto.GameId; }
        }

        /// <summary>
        /// FEN representation of the game
        /// </summary>
        public string Fen
        {
            get { return m_gameDto.Fen; }
        }

        /// <summary>
        /// True if the game is over (for any reason)
        /// </summary>
        public bool GameOver
        {
            get { return m_gameDto.GameOver; }
        }

        public DateTime CreationDate
        {
            get { return m_gameDto.CreationDate; }
        }

        public DateTime CompletionDate
        {
            get { return m_gameDto.CompletionDate; }
        }

        public UserProfile UserProfileBlack
        {
            get { return m_gameDto.UserProfileBlack; }
        }

        public UserProfile UserProfileWhite
        {
            get { return m_gameDto.UserProfileWhite; }
        }

        /// <summary>
        /// Whether a draw may be claimed due to the 50-move rule or the threefold repetition rule.
        /// </summary>
        public bool MayClaimDraw
        {
            get { return m_gameDto.MayClaimDraw; }
        }

        /// <summary>
        /// The move last played in PGN short algebraic format, eg "Re4", "e8(=Q)", "Qa1#"
        /// </summary>
        public string LastMove
        {
            get { return m_gameDto.LastMove; }
        }

        public IClock Clock
        {
            get { return m_gameManager.Clock(m_gameDto.GameId); }
        }

        /// <summary>
        /// The string "b" or "w" or "", representing the colour being played by userName in this game.
        /// </summary>
        /// <param name="userName"></param>
        /// <returns></returns>
        public string CurrentPlayerColor(string userName)
        {
            if (m_gameDto.UserProfileBlack.UserName.Equals(userName, StringComparison.InvariantCultureIgnoreCase))
                return "b";
            if (m_gameDto.UserProfileWhite.UserName.Equals(userName, StringComparison.InvariantCultureIgnoreCase))
                return "w";

            return String.Empty;
        }

        /// <summary>
        /// The winner's user profile. May be null if there is no winner (game not over, game was a draw)
        /// </summary>
        public UserProfile UserProfileWinner
        {
            get { return m_gameDto.UserProfileWinner; }
        }
    }
}