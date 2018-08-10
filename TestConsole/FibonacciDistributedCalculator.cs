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
    internal class FibonacciDistributedCalculator
    {
        private const long InitialFibonacciNumber = 1;
        private const long PredefinedCalculationEndValue = -1;

        private long _previousFibonacciNumber = InitialFibonacciNumber;

        private readonly string _apiUrl;
        private readonly IAdvancedBus _bus;

        public FibonacciDistributedCalculator(string apiUrl, IAdvancedBus bus)
        {
            _apiUrl = apiUrl;
            _bus = bus;
        }

        public void StartCalculation()
        {
            var calculationId = Guid.NewGuid();
            var exchange = _bus.ExchangeDeclare("fibonacciExchange", "direct", durable: true, autoDelete: true);

            var queue = _bus.QueueDeclare();

            var routingId = calculationId.ToString();

            _bus.Bind(exchange, queue, routingId);

            _bus.Consume(queue, (bytes, properties, info) =>
            {
                CalculateNextNumber(bytes, calculationId);
            });

            SendNumber(InitialFibonacciNumber, calculationId);
        }

        private void CalculateNextNumber(byte[] bytes, Guid calculationId)
        {
            var receivedFibonacciNumber = BitConverter.ToInt64(bytes, 0);

            var identificatioString = $"calculationId: {calculationId} | threadId: {Thread.CurrentThread.ManagedThreadId} | ";

            if (receivedFibonacciNumber == PredefinedCalculationEndValue)
            {
                Console.WriteLine(identificatioString + "calculation was ended by WebAPI");
                return;
            }
            
            Console.WriteLine(identificatioString + "received " + receivedFibonacciNumber);

            long newFibonacciNumber;
            try
            {
                newFibonacciNumber = checked(_previousFibonacciNumber + receivedFibonacciNumber);
            }
            catch (OverflowException)
            {
                Console.WriteLine(identificatioString + "ended calculation because of overflow");
                return;
            }

            Console.WriteLine(identificatioString + "calculated " + newFibonacciNumber);

            _previousFibonacciNumber = newFibonacciNumber;

            SendNumber(newFibonacciNumber, calculationId);
        }

        private void SendNumber(long number, Guid calculationId)
        {
            var path = $"fibonacciCalculation/{calculationId}/calculateNextNumber";

            var message = GetMessage(number, path);

            using (var apiClient = new HttpClient())
            {
                apiClient.BaseAddress = new Uri(_apiUrl);
                apiClient.SendAsync(message).Wait();
            }
        }

        private HttpRequestMessage GetMessage(long number, string path)
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
