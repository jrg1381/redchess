using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Threading;
using Microsoft.Azure;
using Microsoft.ServiceBus;
using Microsoft.ServiceBus.Messaging;
using Microsoft.WindowsAzure;
using Microsoft.WindowsAzure.ServiceRuntime;
using Newtonsoft.Json;
using RedChess.MessageQueue;
using RedChess.MessageQueue.Messages;

namespace AnalysisWorker
{
    public class WorkerRole : RoleEntryPoint
    {
        // The name of your queue
        private const string c_queueName = QueueManagerFactory.QueueName;

        // QueueClient is thread-safe. Recommended that you cache 
        // rather than recreating it on every request
        QueueClient m_client;
        readonly ManualResetEvent m_completedEvent = new ManualResetEvent(false);

        public override void Run()
        {
            Trace.WriteLine("Starting processing of messages");

            // Initiates the message pump and callback is invoked for each message that is received, calling close on the client will stop the pump.
            m_client.OnMessage((receivedMessage) =>
                {
                    try
                    {
                        // Process the message
                        Trace.WriteLine("Processing Service Bus message: " + receivedMessage.SequenceNumber.ToString());
                        var body = receivedMessage.GetBody<BasicMessage>();
                        switch (body.MessageType)
                        {
                            case GameEndedMessage.MessageType:
                            {
                                var message = JsonConvert.DeserializeObject<GameEndedMessage>(body.Json);
                                break;
                            }
                            case BestMoveRequestMessage.MessageType:
                            {
                                var message = JsonConvert.DeserializeObject<BestMoveRequestMessage>(body.Json);
                                break;
                            }
                            default:
                            {
                                break;
                            }
                        }

                        receivedMessage.Complete();
                    }
                    catch(Exception e)
                    {
                        Trace.WriteLine("Error: " + e.Message);
                    }
                });

            m_completedEvent.WaitOne();
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
            // Close the connection to Service Bus Queue
            m_client.Close();
            m_completedEvent.Set();
            base.OnStop();
        }
    }
}
