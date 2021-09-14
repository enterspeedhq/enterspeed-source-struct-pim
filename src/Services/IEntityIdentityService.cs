using System;
using Struct.PIM.Api.Models.Asset;
using Struct.PIM.Api.Models.GlobalList;
using Struct.PIM.Api.Models.Product;

namespace Enterspeed.Integration.Struct.Services
{
    public interface IEntityIdentityService
    {
        string GetGlobalListvValueId(GlobalListValue value, string culture);
        string GetAssetId(AssetModel asset);
        string GetAssetId(int id);
        string GetAssetId(string id);
        string GetProductId(ProductModel product, string culture);
        string GetId(int id, string culture = null);
        string GetId(Guid uid, string culture = null);
    }
}
