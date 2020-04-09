using System;
using System.Globalization;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Azure.Messaging.EventHubs;
using Azure.Messaging.EventHubs.Producer;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;

namespace EventGenerator
{
    class Program
    {
        
        private const int MessagesNo = 8;

        private const double Probability = 0.01;
        private static int _cardNumber = -1;
        private static int _sameNumber = 0;
        
        static async Task Main(string[] args)
        {
            IConfiguration configuration = new ConfigurationBuilder()
                                           .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
                                           .Build();

            var connectionString = configuration["EventHubConnectionString"];

            await using var producerClient = new EventHubProducerClient(connectionString);
            
            var rand = new Random();
            
            // Create a batch of events
            using var eventBatch = await producerClient.CreateBatchAsync();

            for (int i = 0; i < MessagesNo; i++)
            {
                int card = 123456789 + rand.Next(0, 888888888);

                // Occasionally generate a fraudulent transaction by reusing a card number
                if (rand.NextDouble() < Probability && _cardNumber != -1)
                {
                    _sameNumber++;
                    card = _cardNumber;
                    _cardNumber = -1;
                }

                // Formulate a transaction
                var transaction = new {
                    transactionId = i+1,
                    transactionTime = DateTime.UtcNow.ToString(CultureInfo.InvariantCulture),
                    deviceId = 12345 + rand.Next(0, 88888),
                    cardNumber = card,
                    amount = rand.Next(1, 20) * 20
                };

                // Occasionally record a card number for later use in generating fraud
                if (rand.NextDouble() < Probability)
                {
                    _cardNumber = transaction.cardNumber;
                }
                
                eventBatch.TryAdd(new EventData(Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(transaction))));
                
                Console.WriteLine($"{transaction.transactionId} transaction generated.");
            }

            // Use the producer client to send the batch of events to the event hub
            await producerClient.SendAsync(eventBatch);
            Console.WriteLine($"A batch of {MessagesNo} events has been published.");
            Console.WriteLine($"Same number transactions {_sameNumber}.");
        }
    }
}