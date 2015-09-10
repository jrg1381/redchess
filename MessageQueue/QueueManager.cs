using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Azure;
using Microsoft.ServiceBus;
using Microsoft.ServiceBus.Messaging;

namespace RedChess.MessageQueue
{
    internal class QueueManager : IQueueManager
    {
        private const string c_queueName = "engine";
        private readonly string m_connectionString;

        public QueueManager()
        {
            m_connectionString = CloudConfigurationManager.GetSetting("Microsoft.ServiceBus.ConnectionString");

            var namespaceManager = NamespaceManager.CreateFromConnectionString(m_connectionString);
            if (!namespaceManager.QueueExists(c_queueName))
            {
                namespaceManager.CreateQueue(c_queueName);
            }
        }

        public void SendMessage()
        {
            var queueClient = QueueClient.CreateFromConnectionString(m_connectionString, "TestQueue");
            queueClient.Send(new BrokeredMessage());
            queueClient.Close();
        }

        public void PostGameEndedMessage(int gameId, string pgnText)
        {
            throw new NotImplementedException();
        }
    }
}
