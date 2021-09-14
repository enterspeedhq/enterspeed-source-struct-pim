using System;
using System.Collections.Generic;
using Enterspeed.Source.Sdk.Api.Models.Properties;
using Struct.PIM.Api.Models.Attribute;
using Attribute = Struct.PIM.Api.Models.Attribute.Attribute;

namespace Enterspeed.Integration.Struct.Services.StructAttributes.StructValueConverters
{
    public class CollectionReferenceAttributeValueConverter : IStructAttributeValueConverter
    {
        private readonly IEntityIdentityService _entityIdentityService;
        private readonly ILocalizationService _localizationService;

        public CollectionReferenceAttributeValueConverter(
            IEntityIdentityService entityIdentityService,
            ILocalizationService localizationService)
        {
            _entityIdentityService = entityIdentityService;
            _localizationService = localizationService;
        }
        public bool IsConverter(Attribute attribute)
        {
            return attribute is CollectionReferenceAttribute;
        }

        public IDictionary<string, IEnterspeedProperty> Convert(Attribute attribute, dynamic value, string culture, bool referencedValue)
        {
            if (!(attribute is CollectionReferenceAttribute collectionReferenceAttribute))
            {
                return null;
            }

            var output = new Dictionary<string, IEnterspeedProperty>();
            if (collectionReferenceAttribute.AllowMultiple)
            {
                List<Guid> references = _localizationService.GetLocalizedValue<List<Guid>>(attribute, value, culture);
                var properties = new List<IEnterspeedProperty>();
                if (references != null)
                {
                    foreach (var id in references)
                    {
                        properties.Add(new StringEnterspeedProperty(_entityIdentityService.GetId(id)));
                    }
                }

                output.Add(attribute.Alias, new ArrayEnterspeedProperty(attribute.Alias, properties.ToArray()));
            }
            else
            {
                Guid referenceId = _localizationService.GetLocalizedValue<Guid>(attribute, value, culture);
                var guidValue = referenceId != Guid.Empty
                    ? _entityIdentityService.GetId(referenceId)
                    : string.Empty;

                output.Add(attribute.Alias, new StringEnterspeedProperty(guidValue));
            }

            return output;
        }
    }
}
