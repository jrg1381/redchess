using System;
using RedChess.ChessCommon;
using RedChess.WebEngine.Repositories.Interfaces;

namespace Chess.Controllers
{
    public class AccessValidator
    {
        private readonly IGameManager m_GameManager;
        private readonly ICurrentUser m_IdentityProvider;
        private static readonly MruCache<Tuple<int, string>, bool> s_Cache;

        static AccessValidator()
        {
            s_Cache = new MruCache<Tuple<int, string>, bool>(25);
        }

        public AccessValidator(IGameManager gameManager, ICurrentUser identityProvider)
        {
            m_GameManager = gameManager;
            m_IdentityProvider = identityProvider;
        }

        public bool MayAccess(int gameId)
        {
            var userName = m_IdentityProvider.CurrentUser;
            bool allowed;
            var gameIdUserIdTuple = new Tuple<int, string>(gameId, userName);

            if (s_Cache.TryGet(gameIdUserIdTuple, out allowed))
            {
                return allowed;
            }
            else
            {
                var game = m_GameManager.FetchGame(gameId);
                if (game == null)
                    return false;
                allowed = (game.UserProfileBlack.UserName == userName || game.UserProfileWhite.UserName == userName);
                s_Cache.Set(gameIdUserIdTuple, allowed);
                return allowed;
            }
        }
    }
}