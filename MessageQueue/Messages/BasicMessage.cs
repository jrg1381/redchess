using Newtonsoft.Json;

namespace RedChess.MessageQueue.Messages
{
    public class BasicMessage
    {
        public string MessageType { get; set; }
        public string Json { get; set; }

        public BasicMessage(string messageType, object messageBody)
        {
            MessageType = messageType;
            Json = JsonConvert.SerializeObject(messageBody);
        }
    }
}