using System.Web.Mvc;

namespace Chess.Controllers
{
	public class HomeController : Controller
	{
		public ActionResult Index(string returnUrl)
		{
		    if (Request.IsAuthenticated)
		    {
		        return RedirectToAction("Index", "Board");
		    }
			ViewBag.ReturnUrl = returnUrl;
			return View();
		}
	}
}