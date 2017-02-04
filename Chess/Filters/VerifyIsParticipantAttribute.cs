using System;
using System.Web.Mvc;
using Chess.Controllers;
using RedChess.WebEngine.Repositories;
using RedChess.WebEngine.Repositories.Interfaces;

namespace Chess.Filters
{
    public class VerifyIsParticipantAttribute : FilterAttribute, IAuthorizationFilter
    {
        private readonly IGameManager m_Manager;
        private readonly ICurrentUser m_IdentityProvider;

        public VerifyIsParticipantAttribute() : this(null, null)
        { }

        public VerifyIsParticipantAttribute(IGameManager manager, ICurrentUser identityProvider)
        {
            m_Manager = manager ?? new GameManager();
            m_IdentityProvider = identityProvider ?? new CurrentUserProvider();
        }

        public void OnAuthorization(AuthorizationContext filterContext)
        {
            int id;
            var idText = filterContext.RequestContext.HttpContext.Request.Params["id"];
            if (idText == null)
                return;

            if (!Int32.TryParse(idText, out id))
                return;

            var accessValidator = new AccessValidator(m_Manager, m_IdentityProvider);
            if (accessValidator.MayAccess(id))
                return;

            var jsonResponse = new JsonResult
            {
                Data = new {success = false, message = "You are not allowed to alter this board", status = "AUTH"}
            };

            filterContext.Result = jsonResponse;
        }
    }
}