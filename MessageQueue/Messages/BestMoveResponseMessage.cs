using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace RedChess.MessageQueue.Messages
{
    public class BestMoveResponseMessage
    {
        public const string MessageType = "BestMoveResponseMessage";

        [JsonProperty(PropertyName = "id")]
        public int GameId { get; set; }
        [JsonProperty(PropertyName = "movenumber")]
        public int MoveNumber { get; set; }
        [JsonProperty(PropertyName = "bestmove")]
        public string BestMove { get; set; }
    }
}
