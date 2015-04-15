using System;
using RedChess.WebEngine.Models;

namespace RedChess.WebEngine.Repositories
{
    public class GameBinding : IGameBinding
    {
        private readonly GameDto m_gameDto;
        private readonly GameManager m_gameManager;

        public GameBinding(GameDto gameDto, GameManager gameManager)
        {
            m_gameDto = gameDto;
            m_gameManager = gameManager;
        }

        public string Description
        {
            get { return m_gameManager.Description(m_gameDto.GameId); }
        }

        public bool ShouldLockUi
        {
            get { return m_gameManager.ShouldLockUi(m_gameDto.GameId); }
        }

        public string Status
        {
            get { return m_gameDto.Status; }
        }

        public int GameId
        {
            get { return m_gameDto.GameId; }
        }

        public string Fen
        {
            get { return m_gameDto.Fen; }
        }

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

        public bool MayClaimDraw
        {
            get { return m_gameDto.MayClaimDraw; }
        }

        public string LastMove
        {
            get { return m_gameDto.LastMove; }
        }

        public IClock Clock
        {
            get { return m_gameManager.Clock(m_gameDto.GameId); }
        }

        public string CurrentPlayerColor(string userName)
        {
            return m_gameManager.CurrentPlayerColor(m_gameDto.GameId, userName);
        }
    }
}