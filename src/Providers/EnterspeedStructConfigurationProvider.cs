using Enterspeed.Integration.Struct.Services;
using Enterspeed.Source.Sdk.Api.Providers;
using Enterspeed.Source.Sdk.Configuration;

namespace Enterspeed.Integration.Struct.Providers
{
    public class EnterspeedStructConfigurationProvider : IEnterspeedConfigurationProvider
    {
        private readonly IEnterspeedConfigurationService _enterspeedConfigurationService;
        public EnterspeedStructConfigurationProvider(
            IEnterspeedConfigurationService enterspeedConfigurationService)
        {
            _enterspeedConfigurationService = enterspeedConfigurationService;
        }
        public EnterspeedConfiguration Configuration => _enterspeedConfigurationService.GetConfiguration();
    }
}
