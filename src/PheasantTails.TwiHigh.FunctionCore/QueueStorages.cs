using Azure.Storage.Queues;
using System.Text;
using System.Text.Json;
using static PheasantTails.TwiHigh.FunctionCore.StaticStrings;

namespace PheasantTails.TwiHigh.FunctionCore
{
    public static class QueueStorages
    {
        public static Task InsertMessageAsync(string queueName, object messageObject)
        {
            var message = JsonSerializer.Serialize(messageObject);
            return InsertMessageAsync(queueName, message);
        }

        public static async Task InsertMessageAsync(string queueName, string message)
        {
            // Get the connection string from app settings
            var connectionString = Environment.GetEnvironmentVariable(QUEUE_STORAGE_CONNECTION_STRINGS_ENV_NAME);

            // Instantiate a QueueClient which will be used to create and manipulate the queue
            var queueClient = new QueueClient(connectionString, queueName);

            // Create the queue if it doesn't already exist
            await queueClient.CreateIfNotExistsAsync();

            if (await queueClient.ExistsAsync())
            {
                // Send a message to the queue
                var base64str = Convert.ToBase64String(Encoding.UTF8.GetBytes(message));
                await queueClient.SendMessageAsync(base64str);
            }
        }
    }
}
