using System;
using Enterspeed.Integration.Struct.Models.Configuration;
using Enterspeed.Integration.Struct.Providers;
using Enterspeed.Integration.Struct.Repository;
using Enterspeed.Integration.Struct.Services;
using Enterspeed.Integration.Struct.Services.StructAttributes;
using Enterspeed.Source.Sdk.Api.Connection;
using Enterspeed.Source.Sdk.Api.Providers;
using Enterspeed.Source.Sdk.Api.Services;
using Enterspeed.Source.Sdk.Domain.Connection;
using Enterspeed.Source.Sdk.Domain.Services;
using Enterspeed.Source.Sdk.Domain.SystemTextJson;
using Microsoft.Extensions.DependencyInjection;
using Struct.PIM.Api.Client;

namespace Enterspeed.Integration.Struct.Configuration
{
    public static class ServicesExtensions
    {
        public static void UseStruct(this IServiceCollection serviceCollection, Action<EnterspeedConfig> configurer = null)
        {
            serviceCollection.AddSingleton<IEnterspeedConfigurationProvider, EnterspeedStructConfigurationProvider>();
            serviceCollection.AddSingleton<IEnterspeedConfigurationService, EnterspeedConfigurationService>();
            serviceCollection.AddSingleton(x =>
            {
                var enterspeedConfigurationService = x.GetService<IEnterspeedConfigurationService>();
                var enterspeedStructConfiguration = enterspeedConfigurationService.GetConfiguration();
                return new StructPIMApiClient(enterspeedStructConfiguration.StructApiUrl,
                    enterspeedStructConfiguration.StructApiKey);
            });

            serviceCollection.AddSingleton<IEnterspeedConnection>(x =>
            {
                var configurationProvider = x.GetService<IEnterspeedConfigurationProvider>();
                return new EnterspeedConnection(configurationProvider);
            });

            var enterspeedConfig = new EnterspeedConfig();
            configurer?.Invoke(enterspeedConfig);
            serviceCollection.AddSingleton(x => enterspeedConfig);

            foreach (var valueConverter in enterspeedConfig.StructAttributeValueConverters)
            {
                serviceCollection.AddScoped(typeof(IStructAttributeValueConverter), valueConverter);
            }

            serviceCollection.AddScoped<IEntityIdentityService, StructEntityIdentityService>();
            serviceCollection.AddScoped<IEnterspeedPropertyService, EnterpeedPropertyService>();
            serviceCollection.AddScoped<IEnterspeedIngestService, EnterspeedIngestService>();
            serviceCollection.AddScoped<IEnterspeedIngestService, EnterspeedIngestService>();
            serviceCollection.AddScoped<IJsonSerializer, SystemTextJsonSerializer>();
            serviceCollection.AddScoped<ILocalizationService, LocalizationService>();

            // Struct repositories
            serviceCollection.AddScoped<IStructAssetRepository, StructAssetRepository>();
            serviceCollection.AddScoped<IStructLanguageRepository, StructLanguageRepository>();
            serviceCollection.AddScoped<IStructAttributeRepository, StructAttributeRepository>();
        }
    }
}
