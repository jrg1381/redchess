using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace RedChess.MessageQueue.Messages
{
    class GameEndedMessage
    {
        [JsonProperty(PropertyName = "id")]
        public int GameId { get; set; }
        [JsonProperty(PropertyName = "pgn")]
        public string Pgn { get; set; }
    }
}
