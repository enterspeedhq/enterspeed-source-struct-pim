using System;
using System.Collections.Generic;
using System.Linq;
using Enterspeed.Source.Sdk.Api.Models.Properties;
using Microsoft.Extensions.DependencyInjection;
using Struct.PIM.Api.Models.Attribute;
using Attribute = Struct.PIM.Api.Models.Attribute.Attribute;

namespace Enterspeed.Integration.Struct.Services.StructAttributes.StructValueConverters
{
    public class FixedListAttributeValueConverter : IStructAttributeValueConverter
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly ILocalizationService _localizationService;

        public FixedListAttributeValueConverter(
        IServiceProvider serviceProvider,
        ILocalizationService localizationService)
        {
            _serviceProvider = serviceProvider;
            _localizationService = localizationService;
        }

        public bool IsConverter(Attribute attribute)
        {
            return attribute is FixedListAttribute;
        }

        public IDictionary<string, IEnterspeedProperty> Convert(Attribute attribute, dynamic value, string culture, bool referencedValue)
        {
            if (!(attribute is FixedListAttribute fixedListAttribute) || fixedListAttribute.ReferencedAttribute == null)
            {
                return null;
            }

            var propertyService = _serviceProvider.GetService<IEnterspeedPropertyService>();
            var output = new Dictionary<string, IEnterspeedProperty>();

            if (fixedListAttribute.AllowMultipleValues)
            {
                var properties = new List<IEnterspeedProperty>();

                if (referencedValue)
                {
                    List<string> localizedValues = _localizationService.GetLocalizedValue<List<string>>(fixedListAttribute, value, culture);
                    if (localizedValues != null)
                    {
                        foreach (var localizedValue in localizedValues)
                        {
                            properties.Add(new StringEnterspeedProperty(localizedValue));
                        }
                    }
                }
                else
                {
                    List<dynamic> localizedValues = _localizationService.GetLocalizedValue<List<dynamic>>(fixedListAttribute.ReferencedAttribute, value, culture);
                    foreach (var localizedValue in localizedValues)
                    {
                        var itemProperties = new Dictionary<string, IEnterspeedProperty>();
                        var subAttributeProperties = propertyService.GetProperties(
                            fixedListAttribute.ReferencedAttribute, localizedValue, culture, referencedValue);
                        if (subAttributeProperties != null)
                        {
                            foreach (var attr in subAttributeProperties)
                            {
                                itemProperties[attr.Key] = attr.Value;
                            }
                        }

                        properties.Add(new ObjectEnterspeedProperty(itemProperties));
                    }
                }

                output.Add(attribute.Alias, new ArrayEnterspeedProperty(attribute.Alias, properties.ToArray()));
            }
            else
            {
                if (referencedValue)
                {
                    string localizedValue = _localizationService.GetLocalizedValue<string>(fixedListAttribute.ReferencedAttribute, value, culture);
                    output.Add(attribute.Alias, new StringEnterspeedProperty(localizedValue));
                }
                else
                {
                    var localizedValue =
                        _localizationService.GetLocalizedValue<dynamic>(fixedListAttribute.ReferencedAttribute, value,
                            culture);

                    var subAttributeProperties = propertyService.GetProperties(
                        fixedListAttribute.ReferencedAttribute,
                        localizedValue,
                        culture,
                        referencedValue);

                    if (subAttributeProperties != null)
                    {
                        var itemProperties = new Dictionary<string, IEnterspeedProperty>();

                        foreach (var attr in subAttributeProperties)
                        {
                            itemProperties[attr.Key] = attr.Value;
                        }

                        if (itemProperties.Any())
                        {
                            output.Add(attribute.Alias, new ObjectEnterspeedProperty(itemProperties));
                        }
                    }
                }
            }

            return output;
        }
    }
}
