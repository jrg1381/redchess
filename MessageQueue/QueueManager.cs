using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Azure;
using Microsoft.ServiceBus;
using Microsoft.ServiceBus.Messaging;
using Newtonsoft.Json;
using RedChess.MessageQueue.Messages;

namespace RedChess.MessageQueue
{
    internal class QueueManager : IQueueManager, IDisposable
    {
        private const string c_queueName = "engine";
        private readonly string m_connectionString;
        private readonly QueueClient m_queueClient;

        internal QueueManager(string connectionString)
        {
            m_connectionString = connectionString;
            m_queueClient = QueueClient.CreateFromConnectionString(m_connectionString, c_queueName);
        }

        private void SendMessage(object message)
        {
            m_queueClient.Send(new BrokeredMessage(message));
        }

        public object PeekQueue()
        {
            var queueHolder = new {messages = new List<string>()};
            var queueClient = QueueClient.CreateFromConnectionString(m_connectionString, c_queueName);

            foreach (var message in queueClient.PeekBatch(5))
            {
                queueHolder.messages.Add(message.GetBody<BasicMessage>().Json);
            }

            queueClient.Close();
            return queueHolder;
        }

        public void PostGameEndedMessage(int gameId, string pgnText)
        {
            var message = new GameEndedMessage {GameId = gameId, Pgn = pgnText};
            SendMessage(new BasicMessage(GameEndedMessage.MessageType, message));
        }

        public void Dispose()
        {
            m_queueClient.Close();
        }
    }
}
