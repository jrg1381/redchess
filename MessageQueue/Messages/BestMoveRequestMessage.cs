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
        public string Move { get; set; }
        [JsonProperty(PropertyName = "movenumber")]
        public int MoveNumber { get; set; }
    }
}