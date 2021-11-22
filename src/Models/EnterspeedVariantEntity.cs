using System.Collections.Generic;
using Enterspeed.Integration.Struct.Services;
using Enterspeed.Source.Sdk.Api.Models;
using Enterspeed.Source.Sdk.Api.Models.Properties;
using Struct.PIM.Api.Models.Attribute;
using Struct.PIM.Api.Models.Variant;

namespace Enterspeed.Integration.Struct.Models
{
    public class EnterspeedVariantEntity : IEnterspeedEntity
    {
        private readonly VariantModel _value;
        private readonly string _culture;
        private readonly IEntityIdentityService _entityIdentityService;

        public EnterspeedVariantEntity(
            VariantModel value,
            Dictionary<Attribute, dynamic> attributeValues,
            string culture,
            IEntityIdentityService entityIdentityService,
            IEnterspeedPropertyService propertyService)
        {
            _value = value;
            _culture = culture;
            _entityIdentityService = entityIdentityService;
            Properties = propertyService.GetProperties(_value, attributeValues, _culture);
        }

        public string Id => _entityIdentityService.GetVariantId(_value, _culture);
        public string Type => "structVariant";
        public string Url => null;
        public string[] Redirects => null;
        public string ParentId => _entityIdentityService.GetProductId(_value.ProductId, _culture);
        public IDictionary<string, IEnterspeedProperty> Properties { get; }
    }
}
