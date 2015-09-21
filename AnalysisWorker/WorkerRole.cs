using System;
using System.Diagnostics;
using System.Net;
using System.Threading;
using Microsoft.Azure;
using Microsoft.ServiceBus.Messaging;
using Microsoft.WindowsAzure.ServiceRuntime;
using Newtonsoft.Json;
using RedChess.MessageQueue;
using RedChess.MessageQueue.Messages;
using RedChess.WebEngine.Repositories;

namespace Redchess.AnalysisWorker
{
    public class WorkerRole : RoleEntryPoint
    {
        // The name of your queue
        private const string c_queueName = QueueManagerFactory.QueueName;

        // QueueClient is thread-safe. Recommended that you cache 
        // rather than recreating it on every request
        QueueClient m_client;
        readonly ManualResetEvent m_completedEvent = new ManualResetEvent(false);
        private readonly IQueueManager m_queueManager = QueueManagerFactory.CreateInstance();
        private UciEngineFarm m_engineFarm;

        public override void Run()
        {
            m_engineFarm = new UciEngineFarm();
            Trace.WriteLine("Starting processing of messages");
            var messageOptions = new OnMessageOptions {MaxConcurrentCalls = 4};

            // Initiates the message pump and callback is invoked for each message that is received, calling close on the client will stop the pump.
            m_client.OnMessage(receivedMessage =>
            {
                try
                {
                    // Process the message
                    Trace.WriteLine("Processing Service Bus message: " + receivedMessage.SequenceNumber.ToString());
                    var body = receivedMessage.GetBody<BasicMessage>();
                    var json = body.Json;

                    switch (body.MessageType)
                    {
                        case GameEndedMessage.MessageType:
                        {
                            ProcessGameEndedMessage(json);
                            break;
                        }
                        case BestMoveRequestMessage.MessageType:
                        {
                            ProcessBestMoveRequestMessage(json);
                            break;
                        }
                        case BestMoveResponseMessage.MessageType:
                        {
                            ProcessBestMoveResponseMessage(json);
                            break;
                        }
                        default:
                        {
                            Trace.TraceError("Received unknown message type " + body.MessageType);
                            break;
                        }
                    }

                    receivedMessage.Complete();
                }
                catch (Exception e)
                {
                    var x = e;

                    do
                    {
                        Trace.TraceError("Error: " + x.Message);
                        x = x.InnerException;
                    } while (x != null);

                    receivedMessage.Abandon();
                }
            }, messageOptions);

            m_completedEvent.WaitOne();
        }

        private static void ProcessBestMoveResponseMessage(string json)
        {
            var message = JsonConvert.DeserializeObject<BestMoveResponseMessage>(json);
            var gameManager = new GameManager();
            gameManager.AddAnalysis(message.GameId, message.MoveNumber, message.BestMove);
        }

        private void ProcessBestMoveRequestMessage(string json)
        {
            var message = JsonConvert.DeserializeObject<BestMoveRequestMessage>(json);
            var bestMove = m_engineFarm.BestMove(message.GameId, message.Fen);
            m_queueManager.PostBestMoveResponseMessage(message.GameId, message.MoveNumber, bestMove);
        }

        private void ProcessGameEndedMessage(string json)
        {
            var message = JsonConvert.DeserializeObject<GameEndedMessage>(json);
            m_engineFarm.GameOver(message.GameId);
        }

        public override bool OnStart()
        {
            // Set the maximum number of concurrent connections 
            ServicePointManager.DefaultConnectionLimit = 12;

            var connectionString = CloudConfigurationManager.GetSetting("Microsoft.ServiceBus.ConnectionString");
            // Initialize the connection to Service Bus Queue
            m_client = QueueClient.CreateFromConnectionString(connectionString, c_queueName);
            return base.OnStart();
        }

        public override void OnStop()
        {
            m_engineFarm.Dispose();
            // Close the connection to Service Bus Queue
            m_client.Close();
            m_completedEvent.Set();
            base.OnStop();
        }
    }
}
