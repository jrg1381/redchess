using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using Microsoft.Azure;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Auth;
using Microsoft.WindowsAzure.Storage.Table;

namespace Redchess.AnalysisWorker
{
    class LogTruncator
    {
        const int c_timerIntervalInHours = 3;
        const int c_maxLogAgeInHours = 6;

        private Timer m_timer;
        private readonly CloudStorageAccount m_storageAccount;

        public LogTruncator()
        {
            Trace.WriteLine("Initializing log truncator");
            m_storageAccount = CloudStorageAccount.Parse(CloudConfigurationManager.GetSetting("Microsoft.WindowsAzure.Plugins.Diagnostics.ConnectionString"));
            m_timer = new Timer(TruncateLogsTimerCallback, null, 0, (int)TimeSpan.FromHours(c_timerIntervalInHours).TotalMilliseconds);
        }

        public void TruncateLogsTimerCallback(object obj)
        {
            Trace.WriteLine("Truncating logs");
            TruncateDiagnostics(m_storageAccount, DateTime.UtcNow.Subtract(TimeSpan.FromHours(c_maxLogAgeInHours)));
        }

        private void TruncateDiagnostics(CloudStorageAccount storageAccount, DateTime keepThreshold)
        {
            try
            {
                var tableClient = storageAccount.CreateCloudTableClient();
                var cloudTable = tableClient.GetTableReference("WADLogsTable");

                var filter = TableQuery.GenerateFilterConditionForDate("Timestamp", QueryComparisons.LessThanOrEqual, keepThreshold);
                var query = new TableQuery {FilterString = filter};

                var items = cloudTable.ExecuteQuery(query).ToList();
                var batches = new Dictionary<string, TableBatchOperation>();

                foreach (var entity in items)
                {
                    var tableOperation = TableOperation.Delete(entity);

                    if (!batches.ContainsKey(entity.PartitionKey))
                    {
                        batches.Add(entity.PartitionKey, new TableBatchOperation());
                    }

                    batches[entity.PartitionKey].Add(tableOperation);
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