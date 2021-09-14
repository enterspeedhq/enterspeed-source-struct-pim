using System;
using System.Collections.Generic;
using System.Linq;
using Enterspeed.Source.Sdk.Api.Models.Properties;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json.Linq;
using Struct.PIM.Api.Models.Attribute;
using Attribute = Struct.PIM.Api.Models.Attribute.Attribute;

namespace Enterspeed.Integration.Struct.Services.StructAttributes.StructValueConverters
{
    public class ListAttributeValueConverter : IStructAttributeValueConverter
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly ILocalizationService _localizationService;

        public ListAttributeValueConverter(
            IServiceProvider serviceProvider,
            ILocalizationService localizationService)
        {
            _serviceProvider = serviceProvider;
            _localizationService = localizationService;
        }
        public bool IsConverter(Attribute attribute)
        {
            return attribute is ListAttribute;
        }

        public IDictionary<string, IEnterspeedProperty> Convert(Attribute attribute, dynamic value, string culture, bool referencedValue)
        {
            if (!(attribute is ListAttribute listAttribute) || !(value is JArray values))
            {
                return null;
            }

            var propertyService = _serviceProvider.GetService<IEnterspeedPropertyService>();

            var output = new Dictionary<string, IEnterspeedProperty>();

            var listItems = new List<IEnterspeedProperty>();
            var localizedValues = _localizationService.GetLocalizedValue<List<JToken>>(attribute, value, culture);
            if (localizedValues != null)
            {
                foreach (var localizedValue in localizedValues)
                {
                    var listItemProperties = new Dictionary<string, IEnterspeedProperty>();

                    var subAttributeProperties = propertyService.GetProperties(listAttribute.Template, localizedValue, culture, referencedValue);
                    if (subAttributeProperties != null)
                    {
                        foreach (var attr in subAttributeProperties)
                        {
                            listItemProperties[attr.Key] = attr.Value;
                        }
                    }

                    if (listItemProperties.Any())
                    {
                        listItems.Add(new ObjectEnterspeedProperty(listItemProperties));
                    }
                }
            }

            if (listItems.Any())
            {
                output.Add(attribute.Alias, new ArrayEnterspeedProperty(attribute.Alias, listItems.ToArray()));
            }

            return output;
        }
    }
}
