using System.Collections.Generic;
using System.Linq;
using Enterspeed.Source.Sdk.Api.Models.Properties;
using Struct.PIM.Api.Models.Attribute;

namespace Enterspeed.Integration.Struct.Services.StructAttributes.StructValueConverters
{
    public class MediaAttributeValueConverter : IStructAttributeValueConverter
    {
        private readonly IEntityIdentityService _entityIdentityService;
        private readonly ILocalizationService _localizationService;

        public MediaAttributeValueConverter(
            IEntityIdentityService entityIdentityService,
            ILocalizationService localizationService)
        {
            _entityIdentityService = entityIdentityService;
            _localizationService = localizationService;
        }

        public bool IsConverter(Attribute attribute)
        {
            return attribute is MediaAttribute;
        }

        public IDictionary<string, IEnterspeedProperty> Convert(Attribute attribute, dynamic value, string culture, bool referencedValue)
        {
            if (!(attribute is MediaAttribute mediaAttribute))
            {
                return null;
            }

            var output = new Dictionary<string, IEnterspeedProperty>();

            if (mediaAttribute.AllowMultiselect)
            {
                List<string> localizedMediaIds = _localizationService.GetLocalizedValue<List<string>>(attribute, value, culture);
                if (localizedMediaIds != null && localizedMediaIds.Any())
                {
                    var mediaIdProperties = new List<IEnterspeedProperty>();
                    foreach (var assetId in localizedMediaIds)
                    {
                        mediaIdProperties.Add(new StringEnterspeedProperty(_entityIdentityService.GetAssetId(assetId)));
                    }

                    output.Add(attribute.Alias, new ArrayEnterspeedProperty(attribute.Alias, mediaIdProperties.ToArray()));
                }
            }
            else
            {
                string mediaId = _localizationService.GetLocalizedValue<string>(attribute, value, culture);
                if (!string.IsNullOrWhiteSpace(mediaId))
                {
                    output.Add(attribute.Alias,
                        new StringEnterspeedProperty(_entityIdentityService.GetAssetId(mediaId)));
                }
            }

            return output;
        }
    }
}
