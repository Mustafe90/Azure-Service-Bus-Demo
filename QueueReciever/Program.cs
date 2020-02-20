using Microsoft.Azure.ServiceBus;
using Newtonsoft.Json;
using System;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace QueueReciever
{
    class Pizza
    {
        public int Cost { get; set; }
        public int Id { get; set; }
        public string DavidCustomMessageToClient { get; set; }
    }

    class Program
    {
        const string ServiceBusConnectionString = "Endpoint=sb://davidspizzashop.servicebus.windows.net/" +
            ";SharedAccessKeyName=RootManageSharedAccessKey;" +
            "SharedAccessKey=/OYEyA0DrQX8El/N13SieexTaCpgLpBFQuXqpzrV/9k=";
        const string QueueName = "davidqueue";
        static IQueueClient queueClient;

        static async Task Main(string[] args)
        {
            await RecieveOrder();
        }
        public static async Task RecieveOrder()
        {
            queueClient = new QueueClient(ServiceBusConnectionString, QueueName);

            RegisterMessageHandler();

            Console.Read();

            await queueClient.CloseAsync();
        }
        static async Task ProcessMessagesAsync(Message message, CancellationToken token)
        {
            var decode = Encoding.UTF8.GetString(message.Body);
            var pizza = JsonConvert.DeserializeObject<Pizza>(decode);
            // Process the message.
            Console.WriteLine($"Received message: SequenceNumber:{message.SystemProperties.SequenceNumber} Body: {pizza.Cost} {pizza.DavidCustomMessageToClient}");

            // Complete the message so that it is not received again.
            // This can be done only if the queue Client is created in ReceiveMode.PeekLock mode (which is the default).
            await queueClient.CompleteAsync(message.SystemProperties.LockToken);
        }

        static void RegisterMessageHandler()
        {
            // Configure the message handler options in terms of exception handling, number of concurrent messages to deliver, etc.
            var messageHandlerOptions = new MessageHandlerOptions(ExceptionReceivedHandler)
            {
                // Maximum number of concurrent calls to the callback ProcessMessagesAsync(), set to 1 for simplicity.
                // Set it according to how many messages the application wants to process in parallel.
                MaxConcurrentCalls = 1,

                // Indicates whether the message pump should automatically complete the messages after returning from user callback.
                // False below indicates the complete operation is handled by the user callback as in ProcessMessagesAsync().
                AutoComplete = false,
               
            };

            // Register the message handler function
            queueClient.RegisterMessageHandler(ProcessMessagesAsync, messageHandlerOptions);

        }
        static Task ExceptionReceivedHandler(ExceptionReceivedEventArgs exceptionReceivedEventArgs)
        {
            Console.WriteLine($"Message handler encountered an exception {exceptionReceivedEventArgs.Exception}.");
            var context = exceptionReceivedEventArgs.ExceptionReceivedContext;
            Console.WriteLine("Exception context for troubleshooting:");
            Console.WriteLine($"- Endpoint: {context.Endpoint}");
            Console.WriteLine($"- Entity Path: {context.EntityPath}");
            Console.WriteLine($"- Executing Action: {context.Action}");
            return Task.CompletedTask;
        }
    }
}
