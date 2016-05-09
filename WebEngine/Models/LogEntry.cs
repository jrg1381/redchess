using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
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

#if DEBUG
        private static string LoremIpsum(int length)
        {
            var loremIpsum =
                @"Lorem ipsum dolor sit amet, consectetur adipiscing elit, sed do eiusmod tempor incididunt ut labore et " +
                "dolore magna aliqua. Ut enim ad minim veniam, quis nostrud exercitation ullamco laboris nisi ut aliquip ex" + 
                " ea commodo consequat Duis aute irure dolor in reprehenderit in voluptate velit esse cillum dolore eu fugiat" +
                " nulla pariatur. Excepteur sint occaecat cupidatat non proident, sunt in culpa qui officia deserunt mollit " +
                "anim id est laborum. ";

            if (loremIpsum.Length < length)
            {
                var sb = new StringBuilder(loremIpsum);
                while (sb.Length < length)
                {
                    sb.Append(loremIpsum);
                }
                loremIpsum = sb.ToString();
            }

            return loremIpsum.Substring(0, length);
        }
#endif

        public static IEnumerable<LogEntry> FetchLogEntries(int entryCount)
        {
#if DEBUG
            return new List<LogEntry>
            {
                new LogEntry() {Message = LoremIpsum(1024), PreciseTimeStamp = DateTime.Now},
                new LogEntry() {Message = LoremIpsum(512), PreciseTimeStamp = DateTime.Now.AddSeconds(33)}
            };
#endif

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
                TakeCount = entryCount,
                SelectColumns = new List<string>() { "Message", "Pid", "Tid", "PreciseTimeStamp" }
            };
            query =
                query.Where(TableQuery.GenerateFilterConditionForDate("PreciseTimeStamp",
                    QueryComparisons.GreaterThanOrEqual, recent));

            return table.ExecuteQuery(query).OrderBy(x => x.PreciseTimeStamp);
        }
    }
}