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

        private readonly object m_dictionaryLock = new object();
        private readonly ConcurrentDictionary<int, BlockingCollection<WorkItem>> m_queueForGame; 

        public UciEngineFarm()
        {
            m_queueForGame = new ConcurrentDictionary<int, BlockingCollection<WorkItem>>();
        }

        private void ProcessQueue(BlockingCollection<WorkItem> workItemQueue, UciEngine engine)
        {
            Debug.WriteLine("Starting queue processor thread " + Thread.CurrentThread.ManagedThreadId);

            try
            {
                WorkItem workItem;
                while (workItemQueue.TryTake(out workItem, Timeout.Infinite))
                {
                    try
                    {
                        lock (workItem)
                        {
                            workItem.Result = engine.BestMove(workItem.Fen);
                            Debug.WriteLine("Pulsing caller from thread " + Thread.CurrentThread.ManagedThreadId);
                            Monitor.Pulse(workItem);
                        }
                    }
                    catch (InvalidOperationException)
                    {
                        break;
                    }
                }
            }
            finally
            {
                engine.Dispose();
            }
        }

        public void GameOver(int gameId)
        {
            lock (m_dictionaryLock)
            {
                Debug.WriteLine("Telling queue for game " + gameId + " to CompleteAdding");
                BlockingCollection<WorkItem> queue;
                if (m_queueForGame.TryGetValue(gameId, out queue))
                {
                    queue.CompleteAdding();
                }
            }
        }

        public string BestMove(int gameId, string fen)
        {
            Debug.WriteLine("Calculating best move for gameId " + gameId + " and fen " + fen);

            lock (m_dictionaryLock)
            {
                BlockingCollection<WorkItem> queue;
                if (!m_queueForGame.TryGetValue(gameId, out queue))
                {
                    Debug.WriteLine("Queue not found for this game id, creating worker");
                    queue = new BlockingCollection<WorkItem>();
                    m_queueForGame[gameId] = queue;
                    var engine = new UciEngine();
                    Task.Factory.StartNew(() => ProcessQueue(queue, engine));
                }
            }

            var workItem = new WorkItem {Fen = fen};

            lock (workItem)
            {
                m_queueForGame[gameId].Add(workItem);
                Debug.WriteLine("Waiting for pulse from worker in thread " + Thread.CurrentThread.ManagedThreadId);
                Monitor.Wait(workItem);
                return workItem.Result;
            }
        }

        public void Dispose()
        {
            foreach (var worker in m_queueForGame.Values)
            {
                worker.CompleteAdding();
                worker.Dispose();
            }
        }
    }
}