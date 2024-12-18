﻿using System.Collections.Generic;
using Enterspeed.Source.Sdk.Api.Models.Properties;
using Struct.PIM.Api.Models.Asset;
using Struct.PIM.Api.Models.Attribute;
using Struct.PIM.Api.Models.Catalogue;
using Struct.PIM.Api.Models.Product;
using Struct.PIM.Api.Models.Variant;

namespace Enterspeed.Integration.Struct.Services
{
    public interface IEnterspeedPropertyService
    {
        IDictionary<string, IEnterspeedProperty> GetProperties(Attribute attribute, dynamic value, string culture, bool referencedValue);
        IDictionary<string, IEnterspeedProperty> GetProperties(AssetModel asset);
        IDictionary<string, IEnterspeedProperty> GetProperties(ProductModel product, Dictionary<Attribute, dynamic> attributeValues, string culture);
        IDictionary<string, IEnterspeedProperty> GetProperties(VariantModel variant, Dictionary<Attribute, dynamic> attributeValues, string culture);
        IDictionary<string, IEnterspeedProperty> GetProperties(CategoryModel category, Dictionary<Attribute, dynamic> attributeValues, string culture);
    }
}
