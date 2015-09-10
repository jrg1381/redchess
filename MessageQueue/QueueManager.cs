﻿using System;
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
        }

        private void SendMessage(string message)
        {
            var queueClient = QueueClient.CreateFromConnectionString(m_connectionString, c_queueName);
            queueClient.Send(new BrokeredMessage(message));
            queueClient.Close();
        }

        public object PeekQueue()
        {
            var queueHolder = new {messages = new List<string>()};
            var queueClient = QueueClient.CreateFromConnectionString(m_connectionString, c_queueName);

            foreach (var message in queueClient.PeekBatch(5))
            {
                queueHolder.messages.Add(message.GetBody<string>());
            }

            queueClient.Close();
            return queueHolder;
        }

        public void PostGameEndedMessage(int gameId, string pgnText)
        {
            var obj = new {id = gameId, pgn = pgnText};
            SendMessage(JsonConvert.SerializeObject(obj));
        }
    }
}
