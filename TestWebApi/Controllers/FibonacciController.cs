using System.Web.Http;
using TestWebApi.Models;

namespace TestWebApi.Controllers
{
    [RoutePrefix("fibonacci")]
    public class FibonacciController : ApiController
    {
        [Route("calculateNextNumber")]
        public void CalculateNextNumber(CalculateNextNumberRequestModel model)
        {
            
        }
    }
}
