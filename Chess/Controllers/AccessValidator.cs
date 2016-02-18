using System;
using RedChess.ChessCommon;
using RedChess.WebEngine.Repositories.Interfaces;

namespace Chess.Controllers
{
    public class AccessValidator
    {
        private readonly IGameManager m_GameManager;
        private readonly ICurrentUser m_IdentityProvider;
        private static readonly MruCache<Tuple<int, int>, bool> m_Cache;

        static AccessValidator()
        {
            m_Cache = new MruCache<Tuple<int, int>, bool>(25);
        }

        public AccessValidator(IGameManager gameManager, ICurrentUser identityProvider)
        {
            m_GameManager = gameManager;
            m_IdentityProvider = identityProvider;
        }

        public bool MayAccess(int gameId)
        {
            var userId = m_IdentityProvider.CurrentUserId;
            bool allowed;
            var gameIdUserIdTuple = new Tuple<int, int>(gameId, userId);

            if (m_Cache.TryGet(gameIdUserIdTuple, out allowed))
            {
                return allowed;
            }
            else
            {
                var game = m_GameManager.FetchGame(gameId);
                if (game == null)
                    return false;
                allowed = (game.UserProfileBlack.UserId == userId || game.UserProfileWhite.UserId == userId);
                m_Cache.Set(gameIdUserIdTuple, allowed);
                return allowed;
            }
        }
    }
}