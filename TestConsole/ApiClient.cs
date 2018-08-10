using System;
using System.IO;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Test.Common.Models;

namespace TestConsole
{
    public class ApiClient : IDisposable
    {
        private readonly HttpClient _httpClient;

        public ApiClient(string apiUrl)
        {
            _httpClient = new HttpClient { BaseAddress = new Uri(apiUrl) };
        }

        public async Task<InitCalculationResponseModel> InitCalculation()
        {
            var message = GetHttpMessage("fibonacciCalculation", HttpMethod.Post);
            var response = await _httpClient.SendAsync(message);
            return await ProcessHttpResponse<InitCalculationResponseModel>(response);
        }

        public async Task CalculateNextNumber(CalculateNextNumberRequestModel calculateNextNumberRequest, Guid calculationId)
        {
            var message = GetHttpMessage(calculateNextNumberRequest, $"fibonacciCalculation/{calculationId}/calculateNextNumber", HttpMethod.Post);
            await _httpClient.SendAsync(message);
        }

        private async Task<TResponse> ProcessHttpResponse<TResponse>(HttpResponseMessage response)
        {
            var content =  await response.Content.ReadAsStringAsync();

            var serializer = JsonSerializer.Create();

            using (var jsonReader = new JsonTextReader(new StringReader(content)))
            {
                return serializer.Deserialize<TResponse>(jsonReader);
            }
        }

        private HttpRequestMessage GetHttpMessage(object model, string path, HttpMethod httpMethod)
        {
            var message = new HttpRequestMessage(httpMethod, path);
            var serializer = JsonSerializer.Create();
            var stringWriter = new StringWriter();
            using (var textWriter = new JsonTextWriter(stringWriter))
            {
                serializer.Serialize(textWriter, model);
                message.Content = new StringContent(stringWriter.ToString());
                if (httpMethod != HttpMethod.Get)
                {
                    message.Content.Headers.ContentType = new MediaTypeHeaderValue("application/json");
                }
            }

            return message;
        }

        private HttpRequestMessage GetHttpMessage(string path, HttpMethod httpMethod)
        {
            var message = new HttpRequestMessage(httpMethod, path);

            return message;
        }

        public void Dispose()
        {
            _httpClient?.Dispose();
        }
    }
}
