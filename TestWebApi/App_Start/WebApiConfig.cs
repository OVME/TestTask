using System.Web.Http;
using TestWebApi.Infrastructure.ActionFiltering;

namespace TestWebApi
{
    public static class WebApiConfig
    {
        public static void Register(HttpConfiguration config)
        {
            // Web API routes
            config.MapHttpAttributeRoutes();

            // Adding filters
            config.Filters.Add(new ValidateModelStateAttribute());
        }
    }
}
