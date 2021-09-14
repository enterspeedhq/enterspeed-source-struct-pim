using System.Collections.Generic;
using Enterspeed.Source.Sdk.Api.Models.Properties;
using Struct.PIM.Api.Models.Attribute;

namespace Enterspeed.Integration.Struct.Services.StructAttributes.StructValueConverters
{
    public class TextAttributeValueConverter : IStructAttributeValueConverter
    {
        private readonly ILocalizationService _localizationService;

        public TextAttributeValueConverter(ILocalizationService localizationService)
        {
            _localizationService = localizationService;
        }

        public bool IsConverter(Attribute attribute)
        {
            return attribute is TextAttribute;
        }

        public IDictionary<string, IEnterspeedProperty> Convert(Attribute attribute, dynamic value, string culture, bool referencedValue)
        {
            string stringValue = _localizationService.GetLocalizedValue<string>(attribute, value, culture);

            var output = new Dictionary<string, IEnterspeedProperty>();
            output.Add(attribute.Alias, new StringEnterspeedProperty(stringValue));
            return output;
        }
    }
}
