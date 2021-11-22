using System;
using Struct.PIM.Api.Models.Asset;
using Struct.PIM.Api.Models.Catalogue;
using Struct.PIM.Api.Models.GlobalList;
using Struct.PIM.Api.Models.Product;
using Struct.PIM.Api.Models.Variant;

namespace Enterspeed.Integration.Struct.Services
{
    public interface IEntityIdentityService
    {
        string GetGlobalListvValueId(GlobalListValue value, string culture);
        string GetAssetId(AssetModel asset);
        string GetAssetId(int id);
        string GetAssetId(string id);
        string GetProductId(ProductModel product, string culture);
        string GetProductId(int productId, string culture);
        string GetVariantId(VariantModel variant, string culture);
        string GetVariantId(int variantId, string culture);
        string GetCategoryId(CategoryModel category, string culture);
        string GetCategoryId(int categoryId, string culture);
        string GetId(int id, string culture = null);
        string GetId(Guid uid, string culture = null);
    }
}
