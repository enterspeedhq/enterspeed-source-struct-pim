using System.Collections.Generic;
using Enterspeed.Source.Sdk.Api.Models.Properties;
using Struct.PIM.Api.Models.Attribute;

namespace Enterspeed.Integration.Struct.Services.StructAttributes.StructValueConverters
{
    public class BooleanAttributeValueConverter : IStructAttributeValueConverter
    {
        private readonly ILocalizationService _localizationService;

        public BooleanAttributeValueConverter(
            ILocalizationService localizationService)
        {
            _localizationService = localizationService;
        }

        public bool IsConverter(Attribute attribute)
        {
            return attribute is BooleanAttribute;
        }

        public IDictionary<string, IEnterspeedProperty> Convert(Attribute attribute, dynamic value, string culture, bool referencedValue)
        {
            bool localizedValue = _localizationService.GetLocalizedValue<bool>(attribute, value, culture);
            
            var output = new Dictionary<string, IEnterspeedProperty>();
            output.Add(attribute.Alias, new BooleanEnterspeedProperty(localizedValue));

            return output;
        }
    }
}
