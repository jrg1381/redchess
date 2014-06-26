using System.Collections.Generic;
using System.IO;
using System.Web.Mvc;
using Chess.Models;
using RedChess.ParserFactory;

namespace Chess.Controllers
{
    public class PgnController : Controller
    {
        private readonly PgnModel m_model;

        public PgnController()
        {
            m_model = new PgnModel();
        }
        //
        // GET: /Pgn/

        public ActionResult Index()
        {
            return View("PgnViewer", m_model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(PgnModel model, string pgnText)
        {
            m_model.PgnText = pgnText;
            try
            {
                var parser = ParserFactory.GetParser();
                parser.Parse(pgnText, (s,m) => m_model.RecordMove(s,m), s => m_model.ErrorText.Add(s), playGame:true);

                m_model.Tags.Clear();
                foreach (var kvp in parser.Tags)
                {
                    m_model.Tags.Add(kvp.Key, kvp.Value);
                }
            }
            catch (InvalidDataException e)
            {
                m_model.ErrorText.Add(e.Message);
                m_model.ErrorText.Add("PGN told engine to perform an illegal move");
            }

            return View("PgnViewer", m_model);
        }
    }
}
