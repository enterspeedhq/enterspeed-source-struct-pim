using System.Collections.Generic;
using Enterspeed.Integration.Struct.Services;
using Enterspeed.Source.Sdk.Api.Models;
using Enterspeed.Source.Sdk.Api.Models.Properties;
using Struct.PIM.Api.Models.Attribute;
using Struct.PIM.Api.Models.Catalogue;
using Struct.PIM.Api.Models.Variant;

namespace Enterspeed.Integration.Struct.Models
{
    public class EnterspeedCategoryEntity : IEnterspeedEntity
    {
        private readonly CategoryModel _value;
        private readonly string _culture;
        private readonly IEntityIdentityService _entityIdentityService;

        public EnterspeedCategoryEntity(
            CategoryModel value,
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

        public string Id => _entityIdentityService.GetCategoryId(_value, _culture);
        public string Type => "structCategory";
        public string Url => null;
        public string[] Redirects => null;
        public string ParentId => null;
        public IDictionary<string, IEnterspeedProperty> Properties { get; }
    }
}
