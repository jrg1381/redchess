﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Azure;
using Microsoft.ServiceBus;
using Microsoft.ServiceBus.Messaging;
using Newtonsoft.Json;
using RedChess.ChessCommon;
using RedChess.ChessCommon.Interfaces;
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

        public void PostGameEndedMessage(int gameId)
        {
            var message = new GameEndedMessage {GameId = gameId};
            SendMessage(new BasicMessage(GameEndedMessage.MessageType, message));
        }

        public void PostRequestBestMoveMessage(int gameId, int moveId, string fen, string move)
        {
            var message = new BestMoveRequestMessage {GameId = gameId, Fen = fen, MoveNumber = moveId, Move = move};
            SendMessage(new BasicMessage(BestMoveRequestMessage.MessageType, message));
        }

        public void PostBestMoveResponseMessage(int gameId, int moveNumber, IBoardAnalysis bestMove)
        {
            var message = new BestMoveResponseMessage
            {
                GameId = gameId,
                MoveNumber = moveNumber,
                Analysis = new BoardAnalysis(bestMove)
            };

            SendMessage(new BasicMessage(BestMoveResponseMessage.MessageType, message));
        }

        public void Dispose()
        {
            m_queueClient.Close();
        }
    }
}
