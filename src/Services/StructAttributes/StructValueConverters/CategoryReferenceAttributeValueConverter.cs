using System.Collections.Generic;
using Enterspeed.Source.Sdk.Api.Models.Properties;
using Struct.PIM.Api.Models.Attribute;

namespace Enterspeed.Integration.Struct.Services.StructAttributes.StructValueConverters
{
    public class CategoryReferenceAttributeValueConverter : IStructAttributeValueConverter
    {
        private readonly IEntityIdentityService _entityIdentityService;
        private readonly ILocalizationService _localizationService;

        public CategoryReferenceAttributeValueConverter(
            IEntityIdentityService entityIdentityService,
            ILocalizationService localizationService)
        {
            _entityIdentityService = entityIdentityService;
            _localizationService = localizationService;
        }
        public bool IsConverter(Attribute attribute)
        {
            return attribute is CategoryReferenceAttribute;
        }

        public IDictionary<string, IEnterspeedProperty> Convert(Attribute attribute, dynamic value, string culture, bool referencedValue)
        {
            if (!(attribute is CategoryReferenceAttribute categoryReferenceAttribute))
            {
                return null;
            }

            var output = new Dictionary<string, IEnterspeedProperty>();
            if (categoryReferenceAttribute.AllowMultiple)
            {
                var localizedValue = _localizationService.GetLocalizedValue<List<int>>(attribute, value, culture);
                var properties = new List<IEnterspeedProperty>();
                if (localizedValue != null)
                {
                    foreach (var id in localizedValue)
                    {
                        properties.Add(new StringEnterspeedProperty(_entityIdentityService.GetId(id, culture)));
                    }
                }

                output.Add(attribute.Alias, new ArrayEnterspeedProperty(attribute.Alias, properties.ToArray()));
            }
            else
            {
                var localizedValue = _localizationService.GetLocalizedValue<int>(attribute, value, culture);
                var categoryId = localizedValue > 0 ? _entityIdentityService.GetId(localizedValue, culture) : string.Empty;
                output.Add(attribute.Alias, new StringEnterspeedProperty(categoryId));
            }

            return output;
        }
    }
}
