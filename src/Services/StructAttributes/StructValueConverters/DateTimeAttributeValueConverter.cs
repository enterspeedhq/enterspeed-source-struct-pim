using System;
using System.Collections.Generic;
using System.Globalization;
using Enterspeed.Source.Sdk.Api.Models.Properties;
using Struct.PIM.Api.Models.Attribute;
using Attribute = Struct.PIM.Api.Models.Attribute.Attribute;

namespace Enterspeed.Integration.Struct.Services.StructAttributes.StructValueConverters
{
    public class DateTimeAttributeValueConverter : IStructAttributeValueConverter
    {
        private readonly ILocalizationService _localizationService;

        public DateTimeAttributeValueConverter(
            ILocalizationService localizationService)
        {
            _localizationService = localizationService;
        }

        public bool IsConverter(Attribute attribute)
        {
            return attribute is DateTimeAttribute;
        }

        public IDictionary<string, IEnterspeedProperty> Convert(Attribute attribute, dynamic value, string culture, bool referencedValue)
        {
            DateTime localizedValue = _localizationService.GetLocalizedValue<DateTime>(attribute, value, culture);

            var output = new Dictionary<string, IEnterspeedProperty>();
            var dateValue = localizedValue != default
                ? localizedValue.ToString(CultureInfo.InvariantCulture)
                : string.Empty;

            output.Add(attribute.Alias, new StringEnterspeedProperty(dateValue));

            return output;
        }
    }
}
