using RedChess.WebEngine.Repositories.Interfaces;

namespace Chess.Controllers
{
    public class AccessValidator
    {
        private readonly IGameManager m_GameManager;
        private readonly ICurrentUser m_IdentityProvider;

        public AccessValidator(IGameManager gameManager, ICurrentUser identityProvider)
        {
            m_GameManager = gameManager;
            m_IdentityProvider = identityProvider;
        }

        public bool MayAccess(int gameId)
        {
            var game = m_GameManager.FetchGame(gameId);
            var userName = m_IdentityProvider.CurrentUser;

            return (game.UserProfileBlack.UserName == userName || game.UserProfileWhite.UserName == userName);
        }
    }
}