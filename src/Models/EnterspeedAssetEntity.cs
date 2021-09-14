using System.Collections.Generic;
using Enterspeed.Integration.Struct.Services;
using Enterspeed.Source.Sdk.Api.Models;
using Enterspeed.Source.Sdk.Api.Models.Properties;
using Struct.PIM.Api.Models.Asset;

namespace Enterspeed.Integration.Struct.Models
{
    public class EnterspeedAssetEntity : IEnterspeedEntity
    {
        private readonly AssetModel _value;
        private readonly IEntityIdentityService _entityIdentityService;

        public EnterspeedAssetEntity(
            AssetModel value,
            IEntityIdentityService entityIdentityService,
            IEnterspeedPropertyService propertyService)
        {
            _value = value;
            _entityIdentityService = entityIdentityService;
            Properties = propertyService.GetProperties(value);
        }

        public string Id => _entityIdentityService.GetAssetId(_value);
        public string Type => "structAsset";
        public string Url => null;
        public string[] Redirects => null;
        public string ParentId => null;
        public IDictionary<string, IEnterspeedProperty> Properties { get; }
    }
}
