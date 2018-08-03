using System;
using System.IO;
using System.Web.Http;
using TestWebApi.Models;

namespace TestWebApi.Controllers
{
    [RoutePrefix("fibonacciCalculation")]
    public class FibonacciCalculationController : ApiController
    {
        [Route("calculateNextNumber")]
        public void CalculateNextNumber(CalculateNextNumberRequestModel model)
        {
            var currentNumber = model.CurrentNumber;
            int nextNumber;

            var tempDirectory = Path.GetTempPath();
            var calculationId = model.CalculationId;
            var tempFilePath = Path.Combine(tempDirectory, "web_api_" + calculationId.ToString());

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
