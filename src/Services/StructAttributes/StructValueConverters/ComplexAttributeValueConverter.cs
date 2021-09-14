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
    public class ComplexAttributeValueConverter : IStructAttributeValueConverter
    {
        private readonly IServiceProvider _serviceProvider;

        public ComplexAttributeValueConverter(
            IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
        }
        public bool IsConverter(Attribute attribute)
        {
            return attribute is ComplexAttribute;
        }

        public IDictionary<string, IEnterspeedProperty> Convert(Attribute attribute, dynamic value, string culture, bool referencedValue)
        {
            if (!(attribute is ComplexAttribute complexAttribute) || !(value is JObject attributeValues))
            {
                return null;
            }

            var propertyService = _serviceProvider.GetService<IEnterspeedPropertyService>();

            var output = new Dictionary<string, IEnterspeedProperty>();

            var dict = attributeValues
                .Cast<JProperty>()
                .ToDictionary(item => item.Name,
                    item => item.Value.Cast<dynamic>());

            foreach (var subAttribute in complexAttribute.SubAttributes)
            {
                var subAttributeValue = dict.ContainsKey(subAttribute.Alias) ? dict[subAttribute.Alias] : null;
                if (subAttributeValue != null)
                {
                    var subAttributeProperties = propertyService.GetProperties(subAttribute, subAttributeValue, culture, referencedValue);
                    if (subAttributeProperties != null)
                    {
                        foreach (var subAttributeProperty in subAttributeProperties)
                        {
                            output[subAttributeProperty.Key] = subAttributeProperty.Value;
                        }
                    }
                }
            }

            return output;
        }
    }
}
