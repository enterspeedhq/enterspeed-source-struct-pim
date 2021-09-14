using System.Collections.Generic;
using Enterspeed.Integration.Struct.Services;
using Enterspeed.Source.Sdk.Api.Models;
using Enterspeed.Source.Sdk.Api.Models.Properties;
using Struct.PIM.Api.Models.GlobalList;

namespace Enterspeed.Integration.Struct.Models
{
    public class EnterspeedGlobalListEntity : IEnterspeedEntity
    {
        private readonly GlobalListValue _value;
        private readonly string _culture;
        private readonly IEntityIdentityService _entityIdentityService;

        public EnterspeedGlobalListEntity(
            GlobalListValue value,
            GlobalList globalList,
            string culture,
            IEntityIdentityService entityIdentityService,
            IEnterspeedPropertyService propertyService)
        {
            _value = value;
            _culture = culture;
            _entityIdentityService = entityIdentityService;
            Properties = propertyService.GetProperties(globalList.Attribute, value.Value, culture, false);
        }

        public string Id => _entityIdentityService.GetGlobalListvValueId(_value, _culture);
        public string Type => "structGlobalListValue";
        public string Url => null;
        public string[] Redirects => null;
        public string ParentId => null;
        public IDictionary<string, IEnterspeedProperty> Properties { get; }
    }
}
