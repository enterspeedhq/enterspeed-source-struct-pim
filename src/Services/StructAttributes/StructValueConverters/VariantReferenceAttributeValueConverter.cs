using System.Collections.Generic;
using Enterspeed.Source.Sdk.Api.Models.Properties;
using Struct.PIM.Api.Models.Attribute;

namespace Enterspeed.Integration.Struct.Services.StructAttributes.StructValueConverters
{
    public class VariantReferenceAttributeValueConverter : IStructAttributeValueConverter
    {
        private readonly IEntityIdentityService _entityIdentityService;
        private readonly ILocalizationService _localizationService;

        public VariantReferenceAttributeValueConverter(
            IEntityIdentityService entityIdentityService,
            ILocalizationService localizationService)
        {
            _entityIdentityService = entityIdentityService;
            _localizationService = localizationService;
        }
        public bool IsConverter(Attribute attribute)
        {
            return attribute is VariantReferenceAttribute;
        }

        public IDictionary<string, IEnterspeedProperty> Convert(Attribute attribute, dynamic value, string culture, bool referencedValue)
        {
            if (!(attribute is VariantReferenceAttribute variantReferenceAttribute))
            {
                return null;
            }

            var output = new Dictionary<string, IEnterspeedProperty>();
            if (variantReferenceAttribute.AllowMultiple)
            {
                var variantIds = _localizationService.GetLocalizedValue<List<int>>(attribute, value, culture);
                var properties = new List<IEnterspeedProperty>();
                if (variantIds != null)
                {
                    foreach (var id in variantIds)
                    {
                        properties.Add(new StringEnterspeedProperty(_entityIdentityService.GetId(id, culture)));
                    }
                }

                output.Add(attribute.Alias, new ArrayEnterspeedProperty(attribute.Alias, properties.ToArray()));
            }
            else
            {
                var localizedValue = _localizationService.GetLocalizedValue<int>(attribute, value, culture);
                var variantId = localizedValue > 0 ? _entityIdentityService.GetId(localizedValue, culture) : string.Empty;
                output.Add(attribute.Alias, new StringEnterspeedProperty(variantId));
            }

            return output;
        }
    }
}
