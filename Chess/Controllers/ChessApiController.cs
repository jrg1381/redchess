using System.Collections.Generic;
using System.Linq;
using System.Web.Http;
using RedChess.WebEngine.Repositories;

namespace Chess.Controllers
{
    /*
    public class BoardCreationResult
    {
        public string Status { get; set; }
        public string Message { get; set; }
        public int Id { get; set; }
    }

    public class PlayMoveResult
    {
        public string Status { get; set; }
        public string Fen { get; set; }
        public string Message { get; set; }
    }
    */
    public class ChessApiController : ApiController
    {
        private readonly IGameManager m_gameManager;

        public ChessApiController() : this(null)
        {
        }

        public ChessApiController(IGameManager manager = null)
        {
            m_gameManager = manager ?? new GameManager();
        }

        [HttpGet]
        public IDictionary<int, string> Boards()
        {
            var returnedDictionary = new Dictionary<int, string>();
            foreach (var game in m_gameManager.FindAll())
            {
                returnedDictionary.Add(game.GameId, game.Fen);
            }
            return returnedDictionary;
        }

        [HttpGet]
        public string Board(int id)
        {
            return m_gameManager.FetchGame(id).Fen;
        }
    }
}
