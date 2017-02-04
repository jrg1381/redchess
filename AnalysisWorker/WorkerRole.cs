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
    public sealed class WorkerRole : RoleEntryPoint, IDisposable
    {
        // The name of your queue
        private const string c_QueueName = QueueManagerFactory.QueueName;

        // QueueClient is thread-safe. Recommended that you cache 
        // rather than recreating it on every request
        QueueClient m_Client;
        readonly ManualResetEvent m_CompletedEvent = new ManualResetEvent(false);
        private readonly IQueueManager m_QueueManager = QueueManagerFactory.CreateInstance();
        private UciEngineFarm m_EngineFarm;
        private static LogTruncator s_LogTruncator;

        public override void Run()
        {
            m_EngineFarm = new UciEngineFarm();
            Trace.WriteLine("Starting processing of messages");

            var messageOptions = new OnMessageOptions
            {
                MaxConcurrentCalls = 1,
                AutoComplete = false
            };

            messageOptions.ExceptionReceived += (sender, args) =>
            {
                Trace.TraceError($"Failed action : {args.Action}");
                Trace.TraceError("Exception processing message loop {0}", args.Exception);
            };

            // Initiates the message pump and callback is invoked for each message that is received, calling close on the client will stop the pump.
            m_Client.OnMessage(receivedMessage =>
            {
                try
                {
                    // Process the message
                    Trace.WriteLine("Processing Service Bus message: " + receivedMessage.SequenceNumber.ToString());
                    var body = receivedMessage.GetBody<BasicMessage>();
                    var json = body.Json;

                    Trace.WriteLine($"Message type : {body.MessageType}");

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
                        Trace.TraceError("Error: " + x.Message + x.StackTrace);
                        x = x.InnerException;
                    } while (x != null);

                    receivedMessage.Abandon();
                }
            }, messageOptions);

            m_CompletedEvent.WaitOne();
        }

        private static void ProcessBestMoveResponseMessage(string json)
        {
            var message = JsonConvert.DeserializeObject<BestMoveResponseMessage>(json);
            var boardAnalysis = message.Analysis;
            var gameManager = new GameManager();
            gameManager.AddAnalysis(message.GameId, message.MoveNumber, boardAnalysis);
        }

        private void ProcessBestMoveRequestMessage(string json)
        {
            var message = JsonConvert.DeserializeObject<BestMoveRequestMessage>(json);
            var boardAnalysis = m_EngineFarm.EvaluateMove(message.GameId, message.Fen, message.Move);
            m_QueueManager.PostBestMoveResponseMessage(message.GameId, message.MoveNumber, boardAnalysis);
        }

        private void ProcessGameEndedMessage(string json)
        {
            var message = JsonConvert.DeserializeObject<GameEndedMessage>(json);
            Trace.WriteLine("Telling engine farm to delete worker for game "  + message.GameId);
            m_EngineFarm.GameOver(message.GameId);
            Trace.WriteLine("Telling database to recalculate ELO table");
            (new GameManager()).UpdateEloTable();
        }

        public override bool OnStart()
        {
            // Set the maximum number of concurrent connections 
            ServicePointManager.DefaultConnectionLimit = 12;

            var connectionString = CloudConfigurationManager.GetSetting("Microsoft.ServiceBus.ConnectionString");
            // Initialize the connection to Service Bus Queue
            m_Client = QueueClient.CreateFromConnectionString(connectionString, c_QueueName);

#if !DEBUG
            s_logTruncator = new LogTruncator();
#endif
            return base.OnStart();
        }

        public override void OnStop()
        {
            m_EngineFarm.Dispose();
            // Close the connection to Service Bus Queue
            m_Client.Close();
            m_CompletedEvent.Set();
            base.OnStop();
        }

        public void Dispose()
        {
            m_EngineFarm.Dispose();
            m_CompletedEvent.Dispose();
        }
    }
}
