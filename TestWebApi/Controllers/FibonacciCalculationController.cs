using System;
using System.IO;
using System.Web.Http;
using EasyNetQ;
using Test.Common.Models;

namespace TestWebApi.Controllers
{
    [RoutePrefix("fibonacciCalculation")]
    public class FibonacciCalculationController : ApiController
    {
        [Route("{calculationId}/calculateNextNumber")]
        public void CalculateNextNumber(CalculateNextNumberRequestModel model, [FromUri] Guid calculationId)
        {
            // ReSharper disable once PossibleInvalidOperationException
            var currentNumber = model.CurrentNumber;
            long nextNumber;

            var tempDirectory = Path.GetTempPath();
            var tempFilePath = Path.Combine(tempDirectory, "web_api_" + calculationId);

            if (currentNumber == 1)
            {
                nextNumber = 2;
            }
            else
            {
                var previousNumberString = File.ReadAllText(tempFilePath);

                long previousNumber;

                var numberParsed = long.TryParse(previousNumberString, out previousNumber);

                if (!numberParsed)
                {
                    throw new FormatException("Can not parse number from tempFilePath. Possibly data was corrupted.");
                }

                nextNumber = previousNumber + currentNumber;
            }

            File.WriteAllText(tempFilePath, nextNumber.ToString());

            SendResultToQueue(nextNumber, calculationId);
        }

        private void SendResultToQueue(long result, Guid calculationId)
        {
            var routingKey = calculationId.ToString();

            using (var bus = RabbitHutch.CreateBus("host=localhost").Advanced)
            {
                var exchange = bus.ExchangeDeclare("fibonacciExchange", "direct", durable: true, autoDelete: true);

                var messageContent = BitConverter.GetBytes(result);

                bus.Publish(exchange, routingKey, true, new MessageProperties(), messageContent);
            }
        }
    }
}
