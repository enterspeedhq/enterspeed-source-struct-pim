using System.Collections.Generic;
using Enterspeed.Integration.Struct.Models.Struct;
using Enterspeed.Source.Sdk.Domain.Connection;

namespace Enterspeed.Integration.Struct.Services.IngestServices
{
    public interface IAssetIngestService
    {
        List<Response> Create(StructAssetsDto assetsDto);
        List<Response> Create(List<string> assetIds);
        List<Response> Update(StructAssetsDto assetsDto);
        List<Response> Update(List<string> assetIds);
        List<Response> Delete(StructAssetsDto assetsDto);
        List<Response> Delete(List<string> assetIds);
    }
}
