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
using Struct.PIM.Api.Models.Catalogue;
using Struct.PIM.Api.Models.Product;
using Struct.PIM.Api.Models.Variant;
using Attribute = Struct.PIM.Api.Models.Attribute.Attribute;

namespace Enterspeed.Integration.Struct.Services
{
    public class EnterspeedPropertyService : IEnterspeedPropertyService
    {
        private readonly IEnumerable<IStructAttributeValueConverter> _valueConverters;
        private readonly IEntityIdentityService _entityIdentityService;
        private readonly StructPIMApiClient _structPimApiClient;
        private readonly IStructAttributeRepository _structAttributeRepository;

        public EnterspeedPropertyService(
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
            var output = new Dictionary<string, IEnterspeedProperty>
            {
                { "id", new StringEnterspeedProperty(_entityIdentityService.GetAssetId(asset)) },
                { "name", new StringEnterspeedProperty(asset.Name) },
                { "url", new StringEnterspeedProperty(asset.Url) },
                { "extension", new StringEnterspeedProperty(asset.Extension) },
                { "fileType", new StringEnterspeedProperty(asset.FileType) },
                { "created", new StringEnterspeedProperty(asset.Created.ToString(CultureInfo.InvariantCulture)) },
                { "lastModified", new StringEnterspeedProperty(asset.LastModified.ToString(CultureInfo.InvariantCulture)) }
            };
            return output;
        }

        public IDictionary<string, IEnterspeedProperty> GetProperties(ProductModel product, Dictionary<Attribute, dynamic> attributeValues, string culture)
        {
            if (product == null || attributeValues == null || attributeValues.Count <= 0)
            {
                return new Dictionary<string, IEnterspeedProperty>();
            }

            var productAttributeValues = _structPimApiClient.Products.GetProductAttributeValues(product.Id, true).Values;
            var productClassificationsModels = _structPimApiClient.Products.GetProductClassifications(product.Id);
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

            foreach (var productAttribute in attributeValues)
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

            output["classifications"] = GetProperties(productClassificationsModels, culture);

            output["metaData"] = CreateMetaData(product, culture);

            return output;
        }

        public IDictionary<string, IEnterspeedProperty> GetProperties(VariantModel variant, Dictionary<Attribute, dynamic> attributeValues, string culture)
        {
            if (variant == null || attributeValues == null || attributeValues.Count <= 0)
            {
                return new Dictionary<string, IEnterspeedProperty>();
            }

            var output = new Dictionary<string, IEnterspeedProperty>();

            foreach (var variantAttribute in attributeValues)
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

            output["metaData"] = CreateMetaData(variant, culture);

            return output;
        }

        public IDictionary<string, IEnterspeedProperty> GetProperties(CategoryModel category, Dictionary<Attribute, dynamic> attributeValues, string culture)
        {
            if (category == null || attributeValues == null || attributeValues.Count <= 0)
            {
                return new Dictionary<string, IEnterspeedProperty>();
            }
            
            var output = new Dictionary<string, IEnterspeedProperty>();

            foreach (var categoryAttribute in attributeValues)
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

        private IEnterspeedProperty CreateMetaData(ProductModel product, string culture)
        {
            if (!product.Name.TryGetValue(culture, out var displayName))
            {
                displayName = string.Empty;
            }

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
                ["variationDefinitionUid"] = new StringEnterspeedProperty(product.VariationDefinitionUid.HasValue ? product.VariationDefinitionUid.ToString() : string.Empty),
                ["displayName"] = new StringEnterspeedProperty(displayName),
            };

            return new ObjectEnterspeedProperty("metaData", metaData);
        }

        public IEnterspeedProperty GetProperties(List<ProductClassificationModel> productClassifications, string culture)
        {
            var classifications = productClassifications
                .Select(x =>
                        new ObjectEnterspeedProperty(new Dictionary<string, IEnterspeedProperty>
                            {
                                { "id", new StringEnterspeedProperty(_entityIdentityService.GetCategoryId(x.CategoryId, culture)) },
                                { "isPrimary", new BooleanEnterspeedProperty(x.IsPrimary) },
                                { "sortOrder", new NumberEnterspeedProperty(x.SortOrder ?? 0) },
                                { "ownerReference", new StringEnterspeedProperty(x.OwnerReference) }
                            }
                        ))
                .ToArray<IEnterspeedProperty>();

            return new ArrayEnterspeedProperty("classifications", classifications);
        }

        private IEnterspeedProperty CreateMetaData(VariantModel variant, string culture)
        {
            if (!variant.Name.TryGetValue(culture, out var displayName))
            {
                displayName = string.Empty;
            }

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
            if (!category.Name.TryGetValue(culture, out var displayName))
            {
                displayName = string.Empty;
            }

            var metaData = new Dictionary<string, IEnterspeedProperty>
            {
                ["createdBy"] = new StringEnterspeedProperty(category.CreatedBy),
                ["culture"] = new StringEnterspeedProperty(culture),
                ["lastModifiedBy"] = new StringEnterspeedProperty(category.LastModifiedBy),
                ["created"] = new StringEnterspeedProperty(category.Created.ToString(CultureInfo.InvariantCulture)),
                ["id"] = new StringEnterspeedProperty(category.Id.ToString()),
                ["hasChildren"] = new BooleanEnterspeedProperty(category.HasChildren),
                ["lastModified"] = new StringEnterspeedProperty(category.LastModified.ToString(CultureInfo.InvariantCulture)),
                ["displayName"] = new StringEnterspeedProperty(displayName),
            };

            return new ObjectEnterspeedProperty("metaData", metaData);
        }
    }
}
