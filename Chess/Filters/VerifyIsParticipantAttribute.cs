using System;
using System.Web.Mvc;
using Chess.Controllers;
using RedChess.WebEngine.Repositories;
using RedChess.WebEngine.Repositories.Interfaces;

namespace Chess.Filters
{
    public class VerifyIsParticipantAttribute : FilterAttribute, IAuthorizationFilter
    {
        private readonly IGameManager m_manager;
        private readonly ICurrentUser m_identityProvider;

        public VerifyIsParticipantAttribute() : this(null, null)
        { }

        public VerifyIsParticipantAttribute(IGameManager manager, ICurrentUser identityProvider)
        {
            m_manager = manager ?? new GameManager();
            m_identityProvider = identityProvider ?? new CurrentUserImpl();
        }

        public void OnAuthorization(AuthorizationContext filterContext)
        {
            int id;
            var idText = filterContext.RequestContext.HttpContext.Request.Params["id"];
            if (idText == null)
                return;

            if (!Int32.TryParse(idText, out id))
                return;

            var accessValidator = new AccessValidator(m_manager, m_identityProvider);
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