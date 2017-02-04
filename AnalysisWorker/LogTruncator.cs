using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using Microsoft.Azure;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Table;

namespace Redchess.AnalysisWorker
{
    class LogTruncator
    {
        const int c_TimerIntervalInHours = 3;
        const int c_MaxLogAgeInHours = 48;
        const string c_LogTable = "WADLogsTable";

        private Timer m_Timer;
        private readonly CloudStorageAccount m_StorageAccount;

        public LogTruncator()
        {
            Trace.WriteLine("Initializing log truncator");
            m_StorageAccount = CloudStorageAccount.Parse(CloudConfigurationManager.GetSetting("Microsoft.WindowsAzure.Plugins.Diagnostics.ConnectionString"));
            m_Timer = new Timer(TruncateLogsTimerCallback, null, 0, (int)TimeSpan.FromHours(c_TimerIntervalInHours).TotalMilliseconds);
        }

        public void TruncateLogsTimerCallback(object obj)
        {
            Trace.WriteLine($"Truncating logs. Max log age {c_MaxLogAgeInHours} hours.");
            TruncateDiagnostics(m_StorageAccount, DateTime.UtcNow.Subtract(TimeSpan.FromHours(c_MaxLogAgeInHours)));
        }

        private void TruncateDiagnostics(CloudStorageAccount storageAccount, DateTime keepThreshold)
        {
            try
            {
                var tableClient = storageAccount.CreateCloudTableClient();
                var cloudTable = tableClient.GetTableReference(c_LogTable);

                var filter = TableQuery.GenerateFilterConditionForDate("Timestamp", QueryComparisons.LessThanOrEqual, keepThreshold);
                var query = new TableQuery {FilterString = filter};

                var items = cloudTable.ExecuteQuery(query).ToList();
                var batches = new Dictionary<string, TableBatchOperation>();

                foreach (var entity in items)
                {
                    var tableOperation = TableOperation.Delete(entity);
                    // The original code partitioned into batches using this, but it wasn't clear what the advantage of doing so was.
                    // I think it's the only column which is 'indexed' so using it prevents a table scan, but we're not doing a 'where' on it, so...
                    var key = entity.PartitionKey;

                    // Can't do more than 100 operations in a batch
                    while (batches.ContainsKey(key) && batches[key].Count >= 99)
                    {
                        key = key + "0";
                    }

                    if (!batches.ContainsKey(key))
                    {
                        batches.Add(key, new TableBatchOperation());
                    }

                    batches[key].Add(tableOperation);
                }

                foreach (var batch in batches.Values)
                {
                    cloudTable.ExecuteBatch(batch);
                }

            }
            catch (Exception ex)
            {
                Trace.TraceError($"Truncate WADLogsTable exception {ex}", "Error");
            }
        }
    }
}