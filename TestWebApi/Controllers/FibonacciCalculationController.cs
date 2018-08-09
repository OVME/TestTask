using System;
using System.Configuration;
using System.IO;
using System.Web.Http;
using EasyNetQ;
using Test.Common.Models;

namespace TestWebApi.Controllers
{
    [RoutePrefix("fibonacciCalculation")]
    public class FibonacciCalculationController : ApiController
    {
        [HttpPost]
        [Route("{calculationId}/calculateNextNumber")]
        public void CalculateNextNumber(CalculateNextNumberRequestModel model, [FromUri] Guid calculationId)
        {
            // ReSharper disable once PossibleInvalidOperationException
            var currentNumber = model.CurrentNumber;

            var tempFilePath = GetFilePathByCalculationId(calculationId);

            long nextNumber;
            if (currentNumber == 1)
            {
                nextNumber = 2;
            }
            else
            {
                var previousNumber = GetPreviousNumberFromFile(tempFilePath);

                nextNumber = checked(previousNumber + currentNumber);
            }

            WriteNextNumberToFile(tempFilePath, nextNumber);

            SendResultToQueue(nextNumber, calculationId);
        }

        private string GetFilePathByCalculationId(Guid calculationId)
        {
            var tempDirectory = Path.GetTempPath();
            var tempFilePath = Path.Combine(tempDirectory, "web_api_" + calculationId);
            return tempFilePath;
        }

        private void WriteNextNumberToFile(string tempFilePath, long nextNumber)
        {
            File.WriteAllText(tempFilePath, nextNumber.ToString());
        }

        private long GetPreviousNumberFromFile(string tempFilePath)
        {
            long previousNumber;
            var previousNumberString = File.ReadAllText(tempFilePath);

            var numberParsed = long.TryParse(previousNumberString, out previousNumber);

            if (!numberParsed)
            {
                throw new FormatException("Can not parse number from tempFilePath. Possibly data was corrupted.");
            }
            return previousNumber;
        }

        private void SendResultToQueue(long result, Guid calculationId)
        {
            var rabbitMqConnectionString = ConfigurationManager.AppSettings["rabbitMqConnectionString"];

            var routingKey = calculationId.ToString();

            using (var bus = RabbitHutch.CreateBus(rabbitMqConnectionString).Advanced)
            {
                var exchange = bus.ExchangeDeclare("fibonacciExchange", "direct", durable: true, autoDelete: true);

                var messageContent = BitConverter.GetBytes(result);

                bus.Publish(exchange, routingKey, true, new MessageProperties(), messageContent);
            }
        }
    }
}
