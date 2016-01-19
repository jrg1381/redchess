using System.Web.Mvc;

namespace Chess.Controllers
{
    public class StatsController : Controller
    {
        public ActionResult Index()
        {
            return View("Index");
        }
    }
}