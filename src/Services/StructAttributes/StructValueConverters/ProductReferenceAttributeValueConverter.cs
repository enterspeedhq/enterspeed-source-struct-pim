using System.Collections.Generic;
using Enterspeed.Source.Sdk.Api.Models.Properties;
using Struct.PIM.Api.Models.Attribute;

namespace Enterspeed.Integration.Struct.Services.StructAttributes.StructValueConverters
{
    public class ProductReferenceAttributeValueConverter : IStructAttributeValueConverter
    {
        private readonly IEntityIdentityService _entityIdentityService;
        private readonly ILocalizationService _localizationService;

        public ProductReferenceAttributeValueConverter(
            IEntityIdentityService entityIdentityService,
            ILocalizationService localizationService)
        {
            _entityIdentityService = entityIdentityService;
            _localizationService = localizationService;
        }
        public bool IsConverter(Attribute attribute)
        {
            return attribute is ProductReferenceAttribute;
        }

        public IDictionary<string, IEnterspeedProperty> Convert(Attribute attribute, dynamic value, string culture, bool referencedValue)
        {
            if (!(attribute is ProductReferenceAttribute productReferenceAttribute))
            {
                return null;
            }

            var output = new Dictionary<string, IEnterspeedProperty>();
            if (productReferenceAttribute.AllowMultiple)
            {
                List<int> productIds = _localizationService.GetLocalizedValue<List<int>>(attribute, value, culture);
                var properties = new List<IEnterspeedProperty>();

                if (productIds != null)
                {
                    foreach (var id in productIds)
                    {
                        properties.Add(new StringEnterspeedProperty(_entityIdentityService.GetId(id, culture)));
                    }
                }

                output.Add(attribute.Alias, new ArrayEnterspeedProperty(attribute.Alias, properties.ToArray()));
            }
            else
            {
                int? productId = _localizationService.GetLocalizedValue<int?>(attribute, value, culture);
                var productIdPropertyValue = productId.HasValue
                    ? _entityIdentityService.GetId(productId.Value, culture)
                    : string.Empty;

                output.Add(attribute.Alias, new StringEnterspeedProperty(productIdPropertyValue));
            }

            return output;
        }
    }
}
