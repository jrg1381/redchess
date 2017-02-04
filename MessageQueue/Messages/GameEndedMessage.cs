using Newtonsoft.Json;

namespace RedChess.MessageQueue.Messages
{
    public class GameEndedMessage
    {
        public const string MessageType = "GameEndedMessage";
 
        [JsonProperty(PropertyName = "id")]
        public int GameId { get; set; }
    }
}
