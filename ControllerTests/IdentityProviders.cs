using Chess.Controllers;
using Rhino.Mocks;

namespace RedChess.ControllerTests
{
    static class IdentityProviders
    {
        public static ICurrentUser StubIdentityProviderFor(string username, int userId = 33)
        {
            var identityProvider = MockRepository.GenerateStub<ICurrentUser>();
            identityProvider.Expect(x => x.CurrentUser).Return(username);
            identityProvider.Expect(x => x.CurrentUserId).Return(userId);
            return identityProvider;
        }
    }
}