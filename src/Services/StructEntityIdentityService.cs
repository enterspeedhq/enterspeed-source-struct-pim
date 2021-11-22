using System;
using Struct.PIM.Api.Models.Asset;
using Struct.PIM.Api.Models.Catalogue;
using Struct.PIM.Api.Models.GlobalList;
using Struct.PIM.Api.Models.Product;
using Struct.PIM.Api.Models.Variant;

namespace Enterspeed.Integration.Struct.Services
{
    public class StructEntityIdentityService : IEntityIdentityService
    {
        public string GetGlobalListvValueId(GlobalListValue value, string culture)
        {
            return GetId(value.Uid, culture);
        }

        public string GetAssetId(AssetModel asset)
        {
            return GetAssetId(asset.Id);
        }

        public string GetAssetId(int id)
        {
            return GetAssetId(id.ToString());
        }

        public string GetAssetId(string id)
        {
            return id;
        }

        public string GetProductId(ProductModel product, string culture)
        {
            return GetProductId(product.Id, culture);
        }

        public string GetProductId(int productId, string culture)
        {
            return GetId(productId, culture);
        }

        public string GetVariantId(VariantModel variant, string culture)
        {
            return GetVariantId(variant.Id, culture);
        }

        public string GetVariantId(int variantId, string culture)
        {
            return GetId(variantId, culture);
        }

        public string GetCategoryId(CategoryModel category, string culture)
        {
            return GetCategoryId(category.Id, culture);
        }

        public string GetCategoryId(int categoryId, string culture)
        {
            return GetId(categoryId, culture);
        }

        public string GetId(int id, string culture = null)
        {
            if (string.IsNullOrWhiteSpace(culture))
            {
                return id.ToString();
            }
            
            return $"{id}-{culture}";
        }

        public string GetId(Guid uid, string culture = null)
        {
            if (string.IsNullOrWhiteSpace(culture))
            {
                return uid.ToString();
            }

            return $"{uid}-{culture}";
        }
    }
}
