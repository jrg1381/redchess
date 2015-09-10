using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Azure;
using Microsoft.ServiceBus;
using Microsoft.ServiceBus.Messaging;
using Newtonsoft.Json;

namespace RedChess.MessageQueue
{
    internal class QueueManager : IQueueManager
    {
        private const string c_queueName = "engine";
        private readonly string m_connectionString;

        internal QueueManager(string connectionString)
        {
            m_connectionString = connectionString;
            var namespaceManager = NamespaceManager.CreateFromConnectionString(connectionString);

            if (!namespaceManager.QueueExists(c_queueName))
            {
                namespaceManager.CreateQueue(c_queueName);
            }
        }

        private void SendMessage(string message)
        {
            var queueClient = QueueClient.CreateFromConnectionString(m_connectionString, "TestQueue");
            queueClient.Send(new BrokeredMessage(message));
            queueClient.Close();
        }

        public void PostGameEndedMessage(int gameId, string pgnText)
        {
            var obj = new {id = gameId, pgn = pgnText};
            SendMessage(JsonConvert.SerializeObject(obj));
        }
    }
}
