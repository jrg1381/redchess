using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using Microsoft.Azure;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Table;

namespace RedChess.WebEngine.Models
{
    public class LogEntry : ITableEntity, ILogEntry
    {
        public DateTime PreciseTimeStamp { get; set; }
        public string Message { get; set; }
        public int Pid { get; set; }
        public int Tid { get; set; }

        private static readonly Regex s_MessageRegex = new Regex(@"Message=""(.*)""$");

        public void ReadEntity(IDictionary<string, EntityProperty> properties, OperationContext operationContext)
        {
            var msg = properties["Message"].StringValue;
            var matches = s_MessageRegex.Match(msg);
            Message = matches.Success ? matches.Groups[1].Value : msg;
            Pid = properties["Pid"].Int32Value.Value;
            Tid = properties["Tid"].Int32Value.Value;
            PreciseTimeStamp = properties["PreciseTimeStamp"].DateTime.Value;
        }

        public IDictionary<string, EntityProperty> WriteEntity(OperationContext operationContext)
        {
            throw new NotImplementedException();
        }

        public string PartitionKey { get; set; }
        public string RowKey { get; set; }
        public DateTimeOffset Timestamp { get; set; }
        public string ETag { get; set; }

        public static IEnumerable<LogEntry> FetchLogEntries()
        {
            // Retrieve the storage account from the connection string.
             var storageAccount = CloudStorageAccount.Parse(
                CloudConfigurationManager.GetSetting("StorageConnectionString"));

            // Create the table client.
            var tableClient = storageAccount.CreateCloudTableClient();

            // Create the CloudTable object that represents the logs table
            var table = tableClient.GetTableReference("WADLogsTable");
            var recent = new DateTimeOffset(DateTime.UtcNow.Subtract(TimeSpan.FromDays(1)));
            var query = new TableQuery<LogEntry>
            {
                TakeCount = 250,
                SelectColumns = new List<string>() { "Message", "Pid", "Tid", "PreciseTimeStamp" }
            };
            query =
                query.Where(TableQuery.GenerateFilterConditionForDate("PreciseTimeStamp",
                    QueryComparisons.GreaterThanOrEqual, recent));

            return table.ExecuteQuery(query).OrderBy(x => x.PreciseTimeStamp);
        }
    }
}