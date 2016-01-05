using System;
using System.Web.Mvc;
using Chess.Controllers;
using RedChess.WebEngine.Repositories;

namespace Chess.Filters
{
    public class VerifyIsParticipantAttribute : FilterAttribute, IAuthorizationFilter
    {
        public void OnAuthorization(AuthorizationContext filterContext)
        {
            int id;
            var idText = filterContext.RequestContext.HttpContext.Request.Params["id"];
            if (idText == null)
                return;

            if (!Int32.TryParse(idText, out id))
                return;

            var accessValidator = new AccessValidator(new GameManager(), new CurrentUserImpl());
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