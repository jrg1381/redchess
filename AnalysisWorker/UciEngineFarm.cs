using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;

namespace Redchess.AnalysisWorker
{
    public class UciEngineFarm : IDisposable
    {
        class WorkItem
        {
            internal string Fen { get; set; }
            internal string Result { get; set; }
        }

        private readonly Dictionary<int, BlockingCollection<WorkItem>> m_queueForGame; 

        public UciEngineFarm()
        {
            m_queueForGame = new Dictionary<int, BlockingCollection<WorkItem>>();
        }

        private void ProcessQueue(BlockingCollection<WorkItem> workItemQueue, UciEngineWrapper engine)
        {
            Debug.WriteLine("Starting queue processor thread " + Thread.CurrentThread.ManagedThreadId);
            while (true)
            {
                try
                {
                    WorkItem workItem = null;
                    if (workItemQueue.TryTake(out workItem))
                    {
                        lock (workItem)
                        {
                            workItem.Result = engine.BestMove(workItem.Fen);
                            Debug.WriteLine("Pulsing caller from thread " + Thread.CurrentThread.ManagedThreadId);
                            Monitor.Pulse(workItem);
                        }
                    }
                }
                catch (InvalidOperationException)
                {
                    break;
                }
            }
        }

        public Task<string> BestMove(int gameId, string fen)
        {
            Debug.WriteLine("Calculating best move for gameId " + gameId + " and fen " + fen);
            BlockingCollection<WorkItem> queue;

            if (!m_queueForGame.TryGetValue(gameId, out queue))
            {
                Debug.WriteLine("Queue not found for this game id, creating worker");
                queue = new BlockingCollection<WorkItem>();
                m_queueForGame[gameId] = queue;
                var engine = new UciEngineWrapper();
                Task.Factory.StartNew(() => ProcessQueue(queue, engine));
            }

            var workItem = new WorkItem() {Fen = fen};

            return Task.Factory.StartNew(() =>
            {
                lock (workItem)
                {
                    m_queueForGame[gameId].Add(workItem);
                    Debug.WriteLine("Waiting for pulse from worker in thread " + Thread.CurrentThread.ManagedThreadId);
                    Monitor.Wait(workItem);
                    return workItem.Result;
                }
            });
        }

        public void Dispose()
        {
            foreach (var worker in m_queueForGame.Values)
            {
                worker.Dispose();
            }
        }
    }
}