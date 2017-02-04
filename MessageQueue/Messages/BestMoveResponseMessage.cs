using Newtonsoft.Json;
using RedChess.ChessCommon;

namespace RedChess.MessageQueue.Messages
{
    public class BestMoveResponseMessage
    {
        public const string MessageType = "BestMoveResponseMessage";

        [JsonProperty(PropertyName = "id")]
        public int GameId { get; set; }
        [JsonProperty(PropertyName = "movenumber")]
        public int MoveNumber { get; set; }
        [JsonProperty(PropertyName = "analysis")]
        public BoardAnalysis Analysis { get; set; }
    }
}
