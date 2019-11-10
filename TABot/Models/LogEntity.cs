using Microsoft.Azure.Cosmos.Table;
using System;

namespace TABot.Models
{
    public class LogEntity : TableEntity
    {
        public string UserName { get; set; }
        public string Message { get; set; }
        public string UserId { get; set; }

        public LogEntity(string userId, string userName, string message)
        {
            PartitionKey = "Unkown Questions";
            RowKey = Guid.NewGuid().ToString();

            UserId = userId;
            Message = message;
            UserName = userName;
        }
    }
}
