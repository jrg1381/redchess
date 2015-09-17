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
        private readonly QueueClient m_queueClient;

        internal QueueManager(string connectionString)
        {
            m_queueClient = QueueClient.CreateFromConnectionString(connectionString, QueueManagerFactory.QueueName);
        }

        private void SendMessage(object message)
        {
            m_queueClient.Send(new BrokeredMessage(message));
        }

        public object PeekQueue()
        {
            var queueHolder = new {messages = new List<string>()};

            foreach (var message in m_queueClient.PeekBatch(5))
            {
                queueHolder.messages.Add(message.GetBody<BasicMessage>().Json);
            }

            return queueHolder;
        }

        public void PostGameEndedMessage(int gameId, string pgnText)
        {
            var message = new GameEndedMessage {GameId = gameId, Pgn = pgnText};
            SendMessage(new BasicMessage(GameEndedMessage.MessageType, message));
        }

        public void PostRequestBestMoveMessage(int gameId, string fen)
        {
            var message = new BestMoveRequestMessage {GameId = gameId, Fen = fen};
            SendMessage(new BasicMessage(BestMoveRequestMessage.MessageType, message));
        }

        public void PostBestMoveResponseMessage(int gameId, string bestMove)
        {
            var message = new BestMoveResponseMessage {GameId = gameId, BestMove = bestMove};
            SendMessage(new BasicMessage(BestMoveResponseMessage.MessageType, message));
        }

        public void Dispose()
        {
            m_queueClient.Close();
        }
    }
}
