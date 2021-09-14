using System.Collections.Generic;
using System.Linq;
using Enterspeed.Source.Sdk.Api.Models.Properties;
using Struct.PIM.Api.Models.Attribute;

namespace Enterspeed.Integration.Struct.Services.StructAttributes.StructValueConverters
{
    public class AttributeReferenceAttributeValueConverter : IStructAttributeValueConverter
    {
        private readonly IEntityIdentityService _entityIdentityService;
        private readonly ILocalizationService _localizationService;

        public AttributeReferenceAttributeValueConverter(
            IEntityIdentityService entityIdentityService,
            ILocalizationService localizationService)
        {
            _entityIdentityService = entityIdentityService;
            _localizationService = localizationService;
        }
        public bool IsConverter(Attribute attribute)
        {
            return attribute is AttributeReferenceAttribute;
        }

        public IDictionary<string, IEnterspeedProperty> Convert(Attribute attribute, dynamic value, string culture, bool referencedValue)
        {
            if (!(attribute is AttributeReferenceAttribute collectionReferenceAttribute))
            {
                return null;
            }

            var output = new Dictionary<string, IEnterspeedProperty>();
            if (collectionReferenceAttribute.AllowMultipleValues)
            {
                List<AttributeReference> references =
                    _localizationService.GetLocalizedValue<List<AttributeReference>>(attribute, value, culture);

                var properties = new List<IEnterspeedProperty>();
                if (references != null)
                {
                    foreach (var id in references.SelectMany(x => x.AttributeUidPath).Distinct())
                    {
                        properties.Add(new StringEnterspeedProperty(_entityIdentityService.GetId(id)));
                    }
                }

                output.Add(attribute.Alias, new ArrayEnterspeedProperty(attribute.Alias, properties.ToArray()));
            }
            else
            {
                AttributeReference reference = _localizationService.GetLocalizedValue<AttributeReference>(attribute, value, culture);
                var referenceValue = reference?.AttributeUidPath?.FirstOrDefault() != null
                    ? _entityIdentityService.GetId(reference.AttributeUidPath.FirstOrDefault())
                    : string.Empty;

                output.Add(attribute.Alias, new StringEnterspeedProperty(referenceValue));
            }

            return output;
        }
    }
}
