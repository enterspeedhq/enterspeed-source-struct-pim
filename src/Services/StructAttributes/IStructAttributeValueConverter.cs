using System.Collections.Generic;
using Enterspeed.Source.Sdk.Api.Models.Properties;
using Struct.PIM.Api.Models.Attribute;

namespace Enterspeed.Integration.Struct.Services.StructAttributes
{
    public interface IStructAttributeValueConverter
    {
        bool IsConverter(Attribute attribute);
        IDictionary<string, IEnterspeedProperty> Convert(Attribute attribute, dynamic value, string culture, bool referencedValue);
    }
}
