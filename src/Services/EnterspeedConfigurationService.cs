using System;
using Enterspeed.Integration.Struct.Models.Configuration;

namespace Enterspeed.Integration.Struct.Services
{
    public class EnterspeedConfigurationService : IEnterspeedConfigurationService
    {
        public EnterspeedStructConfiguration GetConfiguration()
        {
            return new EnterspeedStructConfiguration
            {
                ApiKey = Environment.GetEnvironmentVariable("Enterspeed.ApiKey"),
                BaseUrl = Environment.GetEnvironmentVariable("Enterspeed.Endpoint"),
                StructApiKey = Environment.GetEnvironmentVariable("Struct.ApiKey"),
                StructApiUrl = Environment.GetEnvironmentVariable("Struct.Endpoint")
            };
        }
    }
}
