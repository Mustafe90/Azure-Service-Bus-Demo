using Microsoft.Azure.ServiceBus;
using Newtonsoft.Json;
using System;
using System.Text;
using System.Threading.Tasks;

namespace QueueSender
{
    class Pizza{
        public int Cost {get;set;}
        public int Id {get;set;}
        public string DavidCustomMessageToClient {get;set;}
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
           await MessageAsync();
        }

      static async Task MessageAsync()
        {
            queueClient = new QueueClient(ServiceBusConnectionString, QueueName);

            // Send messages.
            try
            {
                //This can contain an
                string messageBody = JsonConvert.SerializeObject(new Pizza {
                    Cost = 9000,
                    Id = 2,
                    DavidCustomMessageToClient = "Yea okay"
                });

                var message = new Message(Encoding.UTF8.GetBytes(messageBody));

                Console.WriteLine($"Sending message: {messageBody}");

                // Send the message to the queue.
                await queueClient.SendAsync(message);

            }
            catch (Exception exception)
            {
                Console.WriteLine($"{DateTime.Now} :: Exception hello: {exception.Message}");
            }

            await queueClient.CloseAsync();
        }
    }
}
