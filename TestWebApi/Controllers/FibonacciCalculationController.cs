using System;
using System.IO;
using System.Web.Http;
using TestWebApi.Models;

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
            int nextNumber;

            var tempDirectory = Path.GetTempPath();
            var tempFilePath = Path.Combine(tempDirectory, "web_api_" + calculationId);

            if (currentNumber == 1)
            {
                nextNumber = 2;
            }
            else
            {
                var previousNumberString = File.ReadAllText(tempFilePath);

                int previousNumber;

                var numberParsed = int.TryParse(previousNumberString, out previousNumber);

                if (!numberParsed)
                {
                    throw new FormatException("Can not parse number from tempFilePath. Possibly data was corrupted.");
                }

                nextNumber = previousNumber + currentNumber;
            }

            File.WriteAllText(tempFilePath, nextNumber.ToString());
        }
    }
}
