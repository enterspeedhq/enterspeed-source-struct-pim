using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Enterspeed.Integration.Struct.Repository;
using Enterspeed.Integration.Struct.Services.StructAttributes;
using Enterspeed.Source.Sdk.Api.Models.Properties;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json.Linq;
using Struct.PIM.Api.Client;
using Struct.PIM.Api.Models.Asset;
using Struct.PIM.Api.Models.Catalogue;
using Struct.PIM.Api.Models.Product;
using Struct.PIM.Api.Models.Variant;
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

            output["metaData"] = CreateMetaData(product, culture);

            return output;
        }

        public IDictionary<string, IEnterspeedProperty> GetProperties(VariantModel variant, string culture)
        {
            if (variant == null)
            {
                return new Dictionary<string, IEnterspeedProperty>();
            }

            var variantAttributeValuesWithReferences = _structPimApiClient.Variants.GetVariantAttributeValues(variant.Id, true).Values;
            var variantAttributeValues = _structPimApiClient.Variants.GetVariantAttributeValues(variant.Id, false).Values;
            var allAttributes = _structAttributeRepository.GetAllAttributes();
            var allAttributesDictionary = allAttributes.ToDictionary(x => x.Alias);

            var variantAttributesWithReferences = new Dictionary<Attribute, dynamic>();
            var variantAttributes = new Dictionary<Attribute, dynamic>();

            foreach (var variantAttributeValue in variantAttributeValuesWithReferences)
            {
                if (!allAttributesDictionary.TryGetValue(variantAttributeValue.Key, out var attribute))
                {
                    continue;
                }

                variantAttributesWithReferences.Add(attribute, variantAttributeValue.Value);
            }

            foreach (var variantAttributeValue in variantAttributeValues)
            {
                if (!allAttributesDictionary.TryGetValue(variantAttributeValue.Key, out var attribute))
                {
                    continue;
                }

                variantAttributes.Add(attribute, variantAttributeValue.Value);
            }

            var output = new Dictionary<string, IEnterspeedProperty>();

            foreach (var variantAttribute in variantAttributesWithReferences)
            {
                Dictionary<string, IEnterspeedProperty> properties = GetProperties(variantAttribute.Key, variantAttribute.Value, culture, true);
                if (properties == null)
                {
                    continue;
                }

                foreach (var property in properties)
                {
                    output[property.Key] = property.Value;
                }
            }

            output["metaData"] = CreateMetaData(variant, culture, variantAttributes);

            return output;
        }

        public IDictionary<string, IEnterspeedProperty> GetProperties(CategoryModel category, string culture)
        {
            if (category == null)
            {
                return new Dictionary<string, IEnterspeedProperty>();
            }

            var categoryAttributeValues = _structPimApiClient.Catalogues.GetCategoryAttributeValues(category.Id, true).Values;
            var allAttributes = _structAttributeRepository.GetAllAttributes().ToDictionary(x => x.Alias);

            var categoryAttributes = new Dictionary<Attribute, dynamic>();

            foreach (var categoryAttributeValue in categoryAttributeValues)
            {
                if (!allAttributes.TryGetValue(categoryAttributeValue.Key, out var attribute))
                {
                    continue;
                }

                categoryAttributes.Add(attribute, categoryAttributeValue.Value);
            }

            var output = new Dictionary<string, IEnterspeedProperty>();

            foreach (var categoryAttribute in categoryAttributes)
            {
                Dictionary<string, IEnterspeedProperty> properties = GetProperties(categoryAttribute.Key, categoryAttribute.Value, culture, true);
                if (properties == null)
                {
                    continue;
                }

                foreach (var property in properties)
                {
                    output[property.Key] = property.Value;
                }
            }

            output["metaData"] = CreateMetaData(category, culture);

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

        private IEnterspeedProperty CreateMetaData(ProductModel product, string culture)
        {
            var metaData = new Dictionary<string, IEnterspeedProperty>
            {
                ["createdBy"] = new StringEnterspeedProperty(product.CreatedBy),
                ["culture"] = new StringEnterspeedProperty(culture),
                ["lastModifiedBy"] = new StringEnterspeedProperty(product.LastModifiedBy),
                ["archiveReason"] = new StringEnterspeedProperty(product.ArchiveReason.HasValue ? product.ArchiveReason.ToString() : string.Empty),
                ["created"] = new StringEnterspeedProperty(product.Created.ToString(CultureInfo.InvariantCulture)),
                ["id"] = new StringEnterspeedProperty(product.Id.ToString()),
                ["isArchived"] = new BooleanEnterspeedProperty(product.IsArchived),
                ["lastModified"] = new StringEnterspeedProperty(product.LastModified.ToString(CultureInfo.InvariantCulture)),
                ["productStructureUid"] = new StringEnterspeedProperty(product.ProductStructureUid.ToString()),
                ["variationDefinitionUid"] = new StringEnterspeedProperty(product.VariationDefinitionUid.HasValue ? product.VariationDefinitionUid.ToString() : string.Empty)

            };

            return new ObjectEnterspeedProperty("metaData", metaData);
        }

        private IEnterspeedProperty CreateMetaData(VariantModel variant, string culture, Dictionary<Attribute, dynamic> variantAttributes)
        {
            var displayName = variant.Id.ToString(); // TOOD

            var metaData = new Dictionary<string, IEnterspeedProperty>
            {
                ["createdBy"] = new StringEnterspeedProperty(variant.CreatedBy),
                ["culture"] = new StringEnterspeedProperty(culture),
                ["lastModifiedBy"] = new StringEnterspeedProperty(variant.LastModifiedBy),
                ["archiveReason"] = new StringEnterspeedProperty(variant.ArchiveReason.HasValue ? variant.ArchiveReason.ToString() : string.Empty),
                ["created"] = new StringEnterspeedProperty(variant.Created.ToString(CultureInfo.InvariantCulture)),
                ["id"] = new StringEnterspeedProperty(variant.Id.ToString()),
                ["isArchived"] = new BooleanEnterspeedProperty(variant.IsArchived),
                ["lastModified"] = new StringEnterspeedProperty(variant.LastModified.ToString(CultureInfo.InvariantCulture)),
                ["productStructureUid"] = new StringEnterspeedProperty(variant.ProductStructureUid.ToString()),
                ["displayName"] = new StringEnterspeedProperty(displayName),

            };

            return new ObjectEnterspeedProperty("metaData", metaData);
        }

        private IEnterspeedProperty CreateMetaData(CategoryModel category, string culture)
        {
            var metaData = new Dictionary<string, IEnterspeedProperty>
            {
                ["createdBy"] = new StringEnterspeedProperty(category.CreatedBy),
                ["culture"] = new StringEnterspeedProperty(culture),
                ["lastModifiedBy"] = new StringEnterspeedProperty(category.LastModifiedBy),
                ["created"] = new StringEnterspeedProperty(category.Created.ToString(CultureInfo.InvariantCulture)),
                ["id"] = new StringEnterspeedProperty(category.Id.ToString()),
                ["hasChildren"] = new BooleanEnterspeedProperty(category.HasChildren),
                ["lastModified"] = new StringEnterspeedProperty(category.LastModified.ToString(CultureInfo.InvariantCulture)),
            };

            return new ObjectEnterspeedProperty("metaData", metaData);
        }
    }
}
