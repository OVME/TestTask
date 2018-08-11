using System;
using System.Configuration;
using System.IO;
using System.Net;
using System.Web.Http;
using EasyNetQ;
using Test.Common.Models;

namespace TestWebApi.Controllers
{
    [RoutePrefix("fibonacciCalculation")]
    public class FibonacciCalculationController : ApiController
    {
        private const long PredefinedCalculationEndValue = -1;
        private const long FirstFibonacciNumber = 0;

        [HttpPost]
        [Route("")]
        public InitCalculationResponseModel InitCalculation()
        {
            var newId = Guid.NewGuid();
            var tempFilePath = GetFilePathByCalculationId(newId);

            WriteNumberToFile(tempFilePath, FirstFibonacciNumber);

            return new InitCalculationResponseModel
            {
                CalculationId = newId
            };
        }

        [HttpPost]
        [Route("{calculationId}/calculateNextNumber")]
        public void CalculateNextNumber(CalculateNextNumberRequestModel model, [FromUri] Guid calculationId)
        {
            var currentNumber = model.CurrentNumber;

            var tempFilePath = GetFilePathByCalculationId(calculationId);

            var previousNumber = GetPreviousNumberFromFile(tempFilePath);

            long nextNumber;
            try
            {
                nextNumber = checked(previousNumber + currentNumber);
            }
            catch (OverflowException)
            {
                SendResultToQueue(PredefinedCalculationEndValue, calculationId);

                return;
            }

            WriteNumberToFile(tempFilePath, nextNumber);

            SendResultToQueue(nextNumber, calculationId);
        }

        private string GetFilePathByCalculationId(Guid calculationId)
        {
            var tempDirectory = Path.GetTempPath();
            var tempFilePath = Path.Combine(tempDirectory, "web_api_" + calculationId);

            return tempFilePath;
        }

        private void WriteNumberToFile(string tempFilePath, long number)
        {
            File.WriteAllText(tempFilePath, number.ToString());
        }

        private long GetPreviousNumberFromFile(string tempFilePath)
        {
            if (!File.Exists(tempFilePath))
            {
                throw new HttpResponseException(HttpStatusCode.NotFound);
            }

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
