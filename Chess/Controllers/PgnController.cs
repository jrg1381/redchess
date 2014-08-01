using System.Collections.Generic;
using System.IO;
using System.Web.Mvc;
using Chess.Models;
using RedChess.ParserFactory;

namespace Chess.Controllers
{
    public class PgnController : Controller
    {
        //
        // GET: /Pgn/

        public ActionResult Index()
        {
            return View("PgnViewer");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(string pgnText)
        {
            var model = new PgnModel();

            try
            {
                var parser = ParserFactory.GetParser();
                parser.Parse(pgnText, model.RecordMove, s => model.ErrorText.Add(s), playGame: true);

                model.Tags.Clear();
                foreach (var kvp in parser.Tags)
                {
                    model.Tags.Add(kvp.Key, kvp.Value);
                }
            }
            catch (InvalidDataException e)
            {
                model.ErrorText.Add(e.Message);
                model.ErrorText.Add("PGN told engine to perform an illegal move");
            }
            finally
            {
                SetTagDefault(model.Tags, "White", "Anonymous");
                SetTagDefault(model.Tags, "Black", "Anonymous");
            }

            return Json(model);
        }

        private static void SetTagDefault(IDictionary<string, string> tags, string key, string defaultValue)
        {
            if (!tags.ContainsKey(key) || tags[key] == null)
                tags[key] = defaultValue;
        }
    }
}
