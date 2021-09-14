using System.Collections.Generic;
using Struct.PIM.Api.Models.Asset;

namespace Enterspeed.Integration.Struct.Repository
{
    public interface IStructAssetRepository
    {
        AssetModel GetAsset(string id);
        List<AssetModel> GetAssets(List<string> ids);
    }
}
