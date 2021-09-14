using System;
using Struct.PIM.Api.Models.Asset;
using Struct.PIM.Api.Models.GlobalList;
using Struct.PIM.Api.Models.Product;

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
            return GetId(product.Id, culture);
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
