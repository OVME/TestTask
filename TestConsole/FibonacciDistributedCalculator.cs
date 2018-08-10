using System;
using System.Threading;
using EasyNetQ;
using Test.Common.Models;

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
            var calculationId = InitializeDistributedCalculation();
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

        private Guid InitializeDistributedCalculation()
        {
            using (var apiClient = new ApiClient(_apiUrl))
            {
                return apiClient.InitCalculation().GetAwaiter().GetResult().CalculationId;
            }
        }

        private void SendNumber(long number, Guid calculationId)
        {
            using (var apiClient = new ApiClient(_apiUrl))
            {
                var requestModel = new  CalculateNextNumberRequestModel
                {
                    CurrentNumber = number
                };

                apiClient.CalculateNextNumber(requestModel, calculationId).Wait();
            }
        }
    }
}
