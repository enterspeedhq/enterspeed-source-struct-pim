using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Enterspeed.Integration.Struct.Repository;
using Enterspeed.Integration.Struct.Services.StructAttributes;
using Enterspeed.Source.Sdk.Api.Models.Properties;
using Microsoft.Extensions.DependencyInjection;
using Struct.PIM.Api.Client;
using Struct.PIM.Api.Models.Asset;
using Struct.PIM.Api.Models.Product;
using Attribute = Struct.PIM.Api.Models.Attribute.Attribute;

namespace Enterspeed.Integration.Struct.Services
{
    public class EnterpeedPropertyService : IEnterspeedPropertyService
    {
        private readonly IEnumerable<IStructAttributeValueConverter> _valueConverters;
        private readonly IEntityIdentityService _entityIdentityService;
        private readonly StructPIMApiClient _structPimApiClient;
        private readonly IStructAttributeRepository _structAttributeRepository;

        public EnterpeedPropertyService(
            IServiceProvider serviceProvider,
            IEntityIdentityService entityIdentityService,
            StructPIMApiClient structPimApiClient,
            IStructAttributeRepository structAttributeRepository)
        {
            _entityIdentityService = entityIdentityService;
            _structPimApiClient = structPimApiClient;
            _structAttributeRepository = structAttributeRepository;
            _valueConverters = serviceProvider.GetServices<IStructAttributeValueConverter>();
        }

        public IDictionary<string, IEnterspeedProperty> GetProperties(Attribute attribute, dynamic value, string culture, bool referencedValue)
        {
            if (value == null || attribute == null)
            {
                return new Dictionary<string, IEnterspeedProperty>();
            }

            var converter =
                _valueConverters.FirstOrDefault(
                    x => x.IsConverter(attribute));

            var convertedValue = converter?.Convert(attribute, value, culture, referencedValue) ?? new Dictionary<string, IEnterspeedProperty>();

            return convertedValue;
        }

        public IDictionary<string, IEnterspeedProperty> GetProperties(AssetModel asset)
        {
            var output = new Dictionary<string, IEnterspeedProperty>();
            output.Add("id", new StringEnterspeedProperty(_entityIdentityService.GetAssetId(asset)));
            output.Add("name", new StringEnterspeedProperty(asset.Name));
            output.Add("url", new StringEnterspeedProperty(asset.Url));
            output.Add("extension", new StringEnterspeedProperty(asset.Extension));
            output.Add("fileType", new StringEnterspeedProperty(asset.FileType));
            output.Add("created", new StringEnterspeedProperty(asset.Created.ToString(CultureInfo.InvariantCulture)));
            output.Add("lastModified", new StringEnterspeedProperty(asset.LastModified.ToString(CultureInfo.InvariantCulture)));
            return output;
        }

        public IDictionary<string, IEnterspeedProperty> GetProperties(ProductModel product, string culture)
        {
            if (product == null)
            {
                return new Dictionary<string, IEnterspeedProperty>();
            }

            var productAttributeValues = _structPimApiClient.Products.GetProductAttributeValues(product.Id, true).Values;
            var allAttributes = _structAttributeRepository.GetAllAttributes().ToDictionary(x => x.Alias);

            var productAttributes = new Dictionary<Attribute, dynamic>();

            foreach (var productAttributeValue in productAttributeValues)
            {
                if (!allAttributes.TryGetValue(productAttributeValue.Key, out var attribute))
                {
                    continue;
                }

                productAttributes.Add(attribute, productAttributeValue.Value);
            }

            var output = new Dictionary<string, IEnterspeedProperty>();

            foreach (var productAttribute in productAttributes)
            {
                Dictionary<string, IEnterspeedProperty> properties = GetProperties(productAttribute.Key, productAttribute.Value, culture, true);
                if (properties == null)
                {
                    continue;
                }

                foreach (var property in properties)
                {
                    output[property.Key] = property.Value;
                }
            }

            return output;
        }

        private IEnterspeedProperty CreateMetaData(Attribute attribute)
        {
            var metaData = new Dictionary<string, IEnterspeedProperty>
            {
                ["attributeType"] = new StringEnterspeedProperty("attributeType", attribute.AttributeType),
            };

            return new ObjectEnterspeedProperty("metaData", metaData);
        }
    }
}
