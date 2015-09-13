using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace RedChess.MessageQueue.Messages
{
    public class GameEndedMessage
    {
        public const string MessageType = "GameEndedMessage";
 
        [JsonProperty(PropertyName = "id")]
        public int GameId { get; set; }
        [JsonProperty(PropertyName = "pgn")]
        public string Pgn { get; set; }
    }

    public class BestMoveRequestMessage
    {
        public const string MessageType = "BestMoveRequestMessage";

        [JsonProperty(PropertyName = "id")]
        public int GameId { get; set; }
        [JsonProperty(PropertyName="fen")]
        public string Fen { get; set; }
    }
}
