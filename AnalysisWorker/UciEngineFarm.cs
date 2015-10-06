using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;
using RedChess.ChessCommon;
using RedChess.ChessCommon.Enumerations;
using RedChess.ChessCommon.Interfaces;

namespace Redchess.AnalysisWorker
{
    class WorkItem
    {
        internal string Fen { get; set; }
        internal string Move { get; set; }
        internal BoardAnalysis Result { get; set; }
    }

    public class UciEngineFarm : IDisposable
    {
        private readonly object m_dictionaryLock = new object();
        private readonly ConcurrentDictionary<int, BlockingCollection<WorkItem>> m_queueForGame;

        public UciEngineFarm()
        {
            m_queueForGame = new ConcurrentDictionary<int, BlockingCollection<WorkItem>>();
        }

        private void ProcessQueue(BlockingCollection<WorkItem> workItemQueue, UciEngine engine)
        {
            Trace.WriteLine("Starting queue processor thread " + Thread.CurrentThread.ManagedThreadId);

            try
            {
                WorkItem workItem;
                while (workItemQueue.TryTake(out workItem, Timeout.Infinite))
                {
                    try
                    {
                        lock (workItem)
                        {
                            engine.Evaluate(workItem); // Modifies workItem
                            Trace.WriteLine("Pulsing caller from thread " + Thread.CurrentThread.ManagedThreadId);
                            Monitor.Pulse(workItem);
                        }
                    }
                    catch (InvalidOperationException)
                    {
                        break;
                    }
                }
            }
            catch (Exception unexpectedException)
            {
                Trace.TraceError(unexpectedException.Message);
                Trace.TraceError(unexpectedException.StackTrace);
            }
            finally
            {
                Trace.WriteLine("No more work for thread " + Thread.CurrentThread.ManagedThreadId);
                lock (m_dictionaryLock)
                {
                    BlockingCollection<WorkItem> self;
                    m_queueForGame.TryRemove(engine.GameId, out self);
                }
                engine.Dispose();
            }
        }

        public void GameOver(int gameId)
        {
            lock (m_dictionaryLock)
            {
                Trace.WriteLine("Telling queue for game " + gameId + " to CompleteAdding");
                BlockingCollection<WorkItem> queue;
                if (m_queueForGame.TryGetValue(gameId, out queue))
                {
                    queue.CompleteAdding();
                    m_queueForGame.TryRemove(gameId, out queue);
                }
            }
        }

        public IBoardAnalysis EvaluateMove(int gameId, string fen, string move)
        {
            Trace.WriteLine("Calculating best move for gameId " + gameId + " and fen " + fen);

            lock (m_dictionaryLock)
            {
                BlockingCollection<WorkItem> queue;
                if (!m_queueForGame.TryGetValue(gameId, out queue))
                {
                    Trace.WriteLine("Queue not found for this game id, creating worker");
                    queue = new BlockingCollection<WorkItem>();
                    m_queueForGame[gameId] = queue;
                    var engine = new UciEngine(gameId);
                    Trace.WriteLine("Worker created successfully");
                    Task.Factory.StartNew(() => ProcessQueue(queue, engine));
                }
            }

            var workItem = new WorkItem {Fen = fen, Move = move};

            lock (workItem)
            {
                m_queueForGame[gameId].Add(workItem);
                Trace.WriteLine("Waiting for pulse from worker in thread " + Thread.CurrentThread.ManagedThreadId);
                Monitor.Wait(workItem);
                return workItem.Result;
            }
        }

        public void Dispose()
        {
            foreach (var worker in m_queueForGame.Values)
            {
                worker.CompleteAdding();
                var timeOut = DateTime.UtcNow.AddSeconds(5);
                while (!worker.IsCompleted && DateTime.UtcNow < timeOut)
                {
                    Thread.Sleep(250);
                }
            }

            foreach (var worker in m_queueForGame.Values)
            {
                worker.Dispose();
            }
        }
    }
}