using System;
using RedChess.ChessCommon.Enumerations;
using RedChess.WebEngine.Models;
using RedChess.WebEngine.Repositories.Interfaces;

namespace RedChess.WebEngine.Repositories
{
    /// <summary>
    /// A GameBinding is a wrapper around a GameDto, which is the .NET representation of the Boards table
    /// in the database. The GameBinding extends the GameDto with some utility properties which make it more
    /// suitable for the role of the model in an MVC view.
    /// </summary>
    public class GameBinding : IGameBinding
    {
        private readonly GameDto m_GameDto;
        private readonly IGameManager m_GameManager;

        public GameBinding(GameDto gameDto, IGameManager gameManager)
        {
            if(gameDto == null) throw new ArgumentException("Null parameter", nameof(gameDto));
            if (gameManager == null) throw new ArgumentException("Null parameter", nameof(gameManager));

            m_GameDto = gameDto;
            m_GameManager = gameManager;
        }

        /// <summary>
        /// A description of the game in "Foo vs Bar" format.
        /// </summary>
        public string Description => $"{m_GameDto.UserProfileWhite.UserName} vs {m_GameDto.UserProfileBlack.UserName}";

        /// <summary>
        /// Returns true if the game has not yet started (both players have not sent a start request)
        /// </summary>
        public bool ShouldLockUi => m_GameManager.ShouldLockUi(m_GameDto.GameId);

        /// <summary>
        /// A string describing the state of the game : game over, check, checkmate, resigned...
        /// </summary>
        public string Status => m_GameDto.Status;

        /// <summary>
        /// Game ID
        /// </summary>
        public int GameId => m_GameDto.GameId;

        /// <summary>
        /// FEN representation of the game
        /// </summary>
        public string Fen => m_GameDto.Fen;

        /// <summary>
        /// True if the game is over (for any reason)
        /// </summary>
        public bool GameOver => m_GameDto.GameOver;

        public DateTime CreationDate => m_GameDto.CreationDate;
        public DateTime CompletionDate => m_GameDto.CompletionDate;
        public UserProfile UserProfileBlack => m_GameDto.UserProfileBlack;
        public UserProfile UserProfileWhite => m_GameDto.UserProfileWhite;

        /// <summary>
        /// Whether a draw may be claimed due to the 50-move rule or the threefold repetition rule.
        /// </summary>
        public bool MayClaimDraw => m_GameDto.MayClaimDraw;

        /// <summary>
        /// The move last played in PGN short algebraic format, eg "Re4", "e8(=Q)", "Qa1#"
        /// </summary>
        public string LastMove => m_GameDto.LastMove;

        public Location LastMoveStart => m_GameDto.LastMoveStart;
        public Location LastMoveEnd => m_GameDto.LastMoveEnd;

        public IClock Clock => m_GameDto.Clock;

        /// <summary>
        /// The string "b" or "w" or "", representing the colour being played by userName in this game.
        /// </summary>
        /// <param name="userName"></param>
        /// <returns></returns>
        public string CurrentPlayerColor(string userName)
        {
            if (m_GameDto.UserProfileBlack.UserName.Equals(userName, StringComparison.InvariantCultureIgnoreCase))
                return "b";
            if (m_GameDto.UserProfileWhite.UserName.Equals(userName, StringComparison.InvariantCultureIgnoreCase))
                return "w";

            return String.Empty;
        }

        /// <summary>
        /// The winner's user profile. May be null if there is no winner (game not over, game was a draw)
        /// </summary>
        public UserProfile UserProfileWinner => m_GameDto.UserProfileWinner;
    }
}