using System.Collections.Generic;
using Enterspeed.Integration.Struct.Services;
using Enterspeed.Source.Sdk.Api.Models;
using Enterspeed.Source.Sdk.Api.Models.Properties;
using Struct.PIM.Api.Models.Product;

namespace Enterspeed.Integration.Struct.Models
{
    public class EnterspeedProductEntity : IEnterspeedEntity
    {
        private readonly ProductModel _value;
        private readonly string _culture;
        private readonly IEntityIdentityService _entityIdentityService;

        public EnterspeedProductEntity(
            ProductModel value,
            string culture,
            IEntityIdentityService entityIdentityService,
            IEnterspeedPropertyService propertyService)
        {
            _value = value;
            _culture = culture;
            _entityIdentityService = entityIdentityService;
            Properties = propertyService.GetProperties(_value, _culture);
        }

        public string Id => _entityIdentityService.GetProductId(_value, _culture);
        public string Type => "structProduct";
        public string Url => null;
        public string[] Redirects => null;
        public string ParentId => null;
        public IDictionary<string, IEnterspeedProperty> Properties { get; }
    }
}
