using System.Collections.Generic;
using Enterspeed.Source.Sdk.Api.Models.Properties;
using Struct.PIM.Api.Models.Attribute;

namespace Enterspeed.Integration.Struct.Services.StructAttributes.StructValueConverters
{
    public class NumberAttributeValueConverter : IStructAttributeValueConverter
    {
        private readonly ILocalizationService _localizationService;

        public NumberAttributeValueConverter(
            ILocalizationService localizationService)
        {
            _localizationService = localizationService;
        }

        public bool IsConverter(Attribute attribute)
        {
            return attribute is NumberAttribute;
        }

        public IDictionary<string, IEnterspeedProperty> Convert(Attribute attribute, dynamic value, string culture, bool referencedValue)
        {
            if (!(attribute is NumberAttribute numberAttribute))
            {
                return null;
            }

            var output = new Dictionary<string, IEnterspeedProperty>();

            double? localizedValue = _localizationService.GetLocalizedValue<double?>(attribute, value, culture);

            if (localizedValue == null)
            {
                return null;
            }

            var valueProperty = new NumberEnterspeedProperty(localizedValue.Value);
            var unitProperty = new StringEnterspeedProperty(numberAttribute.Unit ?? string.Empty);

            var numberProperties = new Dictionary<string, IEnterspeedProperty>();
            numberProperties.Add("value", valueProperty);
            numberProperties.Add("unit", unitProperty);

            output.Add(attribute.Alias, new ObjectEnterspeedProperty(numberProperties));
            return output;
        }
    }
}
