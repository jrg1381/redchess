using System;

namespace Redchess.AnalysisWorker
{
    public interface IUciEngine : IDisposable
    {
        void Evaluate(WorkItem workItem);
        int GameId { get; }
    }
}