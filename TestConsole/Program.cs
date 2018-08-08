using System;
using System.IO;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading;
using EasyNetQ;
using Newtonsoft.Json;
using Test.Common.Models;
using JsonSerializer = Newtonsoft.Json.JsonSerializer;

namespace TestConsole
{
    public class Program
    {
        private static long PreviousFibonacciNumber;

        static void Main(string[] args)
        {
            var initialFibonacciNumber = 1L;

            var calculationId = Guid.NewGuid();
            
            PreviousFibonacciNumber = initialFibonacciNumber;

            Console.WriteLine("threadId" + Thread.CurrentThread.ManagedThreadId);

            using (var bus = RabbitHutch.CreateBus("host=localhost").Advanced)
            {
                var exchange = bus.ExchangeDeclare("fibonacciExchange", "direct", durable: true, autoDelete: true);

                var queue = bus.QueueDeclare();

                var routingId = calculationId.ToString();

                bus.Bind(exchange, queue, routingId);

                bus.Consume(queue, (bytes, properties, info) =>
                {
                    var receivedFibonacciNumber = BitConverter.ToInt32(bytes, 0);

                    Console.WriteLine(receivedFibonacciNumber);

                    var newFibonacciNumber = PreviousFibonacciNumber + receivedFibonacciNumber;

                    SendNumber(newFibonacciNumber, calculationId);

                    Console.WriteLine(newFibonacciNumber);

                    Console.WriteLine("threadId" + Thread.CurrentThread.ManagedThreadId);

                    PreviousFibonacciNumber = newFibonacciNumber;
                });

                SendNumber(initialFibonacciNumber, calculationId);

                Console.ReadLine();
                Console.WriteLine("threadId" + Thread.CurrentThread.ManagedThreadId);
                Console.ReadLine();
            }
        }

        private static void SendNumber(long number, Guid calculationId)
        {
            var path = $"fibonacciCalculation/{calculationId}/calculateNextNumber";

            var message = GetMessage(number, path);

            using (var apiClient = new HttpClient())
            {
                apiClient.BaseAddress = new Uri($"http://localhost:50607/");
                apiClient.SendAsync(message).Wait();
            }
        }

        private static HttpRequestMessage GetMessage(long number, string path)
        {
            var model = new CalculateNextNumberRequestModel
            {
                CurrentNumber = number
            };

            var message = new HttpRequestMessage(HttpMethod.Post, path);
            var serializer = JsonSerializer.Create();
            var stringWriter = new StringWriter();
            using (var textWriter = new JsonTextWriter(stringWriter))
            {
                serializer.Serialize(textWriter, model);
                message.Content = new StringContent(stringWriter.ToString());
                message.Content.Headers.ContentType = new MediaTypeHeaderValue("application/json");
            }

            return message;
        }
    }
}
