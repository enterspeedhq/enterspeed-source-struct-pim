using System.Collections.Generic;
using System.Linq;
using Struct.PIM.Api.Client;
using Struct.PIM.Api.Models.Asset;

namespace Enterspeed.Integration.Struct.Repository
{
    public class StructAssetRepository : IStructAssetRepository
    {
        private readonly StructPIMApiClient _structPimApiClient;

        private readonly Dictionary<string, AssetModel> _cachedAssets = new Dictionary<string, AssetModel>();
        public StructAssetRepository(
            StructPIMApiClient structPimApiClient)
        {
            _structPimApiClient = structPimApiClient;
        }

        public AssetModel GetAsset(string id)
        {
            if (_cachedAssets.ContainsKey(id))
            {
                return _cachedAssets[id];
            }

            var asset = _structPimApiClient.Assets.GetAsset(id);
            if (asset != null)
            {
                _cachedAssets[id] = asset;
            }

            return asset;
        }

        public List<AssetModel> GetAssets(List<string> ids)
        {
            var result = new List<AssetModel>();

            var notCachedIds = new List<string>();
            foreach (var id in ids)
            {
                if (_cachedAssets.ContainsKey(id))
                {
                    result.Add(_cachedAssets[id]);
                }
                else
                {
                    notCachedIds.Add(id);
                }
            }

            if (notCachedIds.Any())
            {
                var assets = _structPimApiClient.Assets.GetAssets(notCachedIds);
                if (assets != null)
                {
                    foreach (var asset in assets)
                    {
                        _cachedAssets[asset.Id] = asset;
                    }
                }
            }

            return result;
        }
    }
}
