using System.Web.Mvc;
using RedChess.WebEngine.Models;

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

        public ActionResult Config()
        {
            return View("Config", LogEntry.FetchLogEntries());
        }
    }
}