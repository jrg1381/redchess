using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Emit;
using System.Web.Mvc;
using Microsoft.Azure;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Table;
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