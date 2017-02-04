using System.Web.Security;
using RedChess.ChessCommon.Interfaces;
using RedChess.WebEngine.Repositories;
using WebMatrix.WebData;

namespace Chess.Controllers
{
    class DefaultWebSecurityProvider : IWebSecurityProvider
    {
        private readonly ICurrentUser m_IdentityProvider;

        public DefaultWebSecurityProvider(ICurrentUser identityProvider = null)
        {
            m_IdentityProvider = identityProvider ?? new CurrentUserProvider();
        }

        public bool Login(string userName, string password, bool persistCookie = false)
        {
            return WebSecurity.Login(userName, password, persistCookie);
        }

        public void Logout()
        {
            WebSecurity.Logout();
        }

        public void CreateUserAndAccount(string userName, string password, object propertyValues)
        {
            WebSecurity.CreateUserAndAccount(userName, password, propertyValues);
        }

        public bool ChangePassword(string name, string oldPassword, string newPassword)
        {
            return WebSecurity.ChangePassword(name, oldPassword, newPassword);
        }

        public void SetAuthCookie(string userName, bool rememberMe)
        {
            FormsAuthentication.SetAuthCookie(userName, rememberMe);
        }

        public void ChangeEmailHash(string hashedEmail)
        {
            (new GameManager()).SetEmailHashForUserId(m_IdentityProvider.CurrentUserId, hashedEmail);
        }
    }
}