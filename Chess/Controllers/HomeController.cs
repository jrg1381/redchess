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

        public ActionResult Register(string returnUrl) => View("Register");

        // This is a bit of a misuse of 'id' parameter
        public ActionResult Config(int? id) => View("Config", LogEntry.FetchLogEntries(id ?? 250));
    }
}