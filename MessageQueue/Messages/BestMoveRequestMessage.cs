using Newtonsoft.Json;

namespace RedChess.MessageQueue.Messages
{
    public class BestMoveRequestMessage
    {
        public const string MessageType = "BestMoveRequestMessage";

        [JsonProperty(PropertyName = "id")]
        public int GameId { get; set; }
        [JsonProperty(PropertyName="fen")]
        public string Fen { get; set; }
        [JsonProperty(PropertyName = "move")]
        public int MoveNumber { get; set; }
    }
}