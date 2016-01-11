namespace RedChess.ChessCommon.Interfaces
{
    public interface IWebSecurityProvider
    {
        bool Login(string userName, string password, bool persistCookie = false);
        void Logout();
        void CreateUserAndAccount(string userName, string password, object propertyValues);
        bool ChangePassword(string name, string oldPassword, string newPassword);
        void SetAuthCookie(string userName, bool rememberMe);
    }
}