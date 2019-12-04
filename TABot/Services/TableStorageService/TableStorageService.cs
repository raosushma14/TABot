using Microsoft.Azure.Cosmos.Table;
using System;
using System.Threading.Tasks;
using TABot.Models;

namespace TABot.Services.TableStorageService
{
    public class TableStorageService
    {
        private string _connectionString;

        public TableStorageService(string connectionString)
        {
            _connectionString = connectionString ?? throw new ArgumentNullException(nameof(connectionString));
        }

        public async Task InsertRecordAsync(LogEntity entity)
        {
            if(entity == null)
            {
                throw new ArgumentNullException();
            }

            var table = await CreateTableAsync(_connectionString, "botLog");

            TableOperation operation = TableOperation.InsertOrMerge(entity);
            TableResult result = await table.ExecuteAsync(operation);
        }

        private async Task<CloudTable> CreateTableAsync(string conn, string tableName)
        {
            string storageConnectionString = conn;

            // Retrieve storage account information from connection string.
            CloudStorageAccount storageAccount = CreateStorageAccount(storageConnectionString);

            // Create a table client for interacting with the table service
            CloudTableClient tableClient = storageAccount.CreateCloudTableClient(new TableClientConfiguration());

            // Create a table client for interacting with the table service 
            CloudTable table = tableClient.GetTableReference(tableName);
            if (await table.CreateIfNotExistsAsync())
            {
                Console.WriteLine("Created Table named: {0}", tableName);
            }
            else
            {
                Console.WriteLine("Table {0} already exists", tableName);
            }

            Console.WriteLine();
            return table;
        }

        private CloudStorageAccount CreateStorageAccount(string conn)
        {
            CloudStorageAccount storageAccount;
            try
            {
                storageAccount = CloudStorageAccount.Parse(conn);
            }
            catch (FormatException)
            {
                Console.WriteLine("Invalid storage account information provided. Please confirm the AccountName and AccountKey are valid in the app.config file - then restart the application.");
                throw;
            }
            catch (ArgumentException)
            {
                Console.WriteLine("Invalid storage account information provided. Please confirm the AccountName and AccountKey are valid in the app.config file - then restart the sample.");
                Console.ReadLine();
                throw;
            }

            return storageAccount;
        }
    }
}
