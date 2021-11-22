using System.Collections.Generic;
using System.Linq;
using System.Net;
using Enterspeed.Integration.Struct.Models;
using Enterspeed.Integration.Struct.Models.Struct;
using Enterspeed.Source.Sdk.Api.Services;
using Enterspeed.Source.Sdk.Domain.Connection;
using MoreLinq;
using Struct.PIM.Api.Client;
using Struct.PIM.Api.Models.Asset;

namespace Enterspeed.Integration.Struct.Services.IngestServices
{
    public class AssetEnterspeedIngestService : IAssetIngestService
    {
        private readonly StructPIMApiClient _structPimApiClient;
        private readonly IEntityIdentityService _entityIdentityService;
        private readonly IEnterspeedPropertyService _enterspeedPropertyService;
        private readonly IEnterspeedIngestService _enterspeedIngestService;

        public AssetEnterspeedIngestService(
            StructPIMApiClient structPimApiClient,
            IEntityIdentityService entityIdentityService,
            IEnterspeedPropertyService enterspeedPropertyService,
            IEnterspeedIngestService enterspeedIngestService)
        {
            _structPimApiClient = structPimApiClient;
            _entityIdentityService = entityIdentityService;
            _enterspeedPropertyService = enterspeedPropertyService;
            _enterspeedIngestService = enterspeedIngestService;
        }

        public List<Response> Create(StructAssetsDto assetsDto)
        {
            return Create(assetsDto?.MediasItems?.Select(x => x.Id.ToString()).ToList());
        }

        public List<Response> Create(List<string> assetIds)
        {
            return CreateOrUpdate(assetIds);
        }

        public List<Response> Update(StructAssetsDto assetsDto)
        {
            return Update(assetsDto?.MediasItems?.Select(x => x.Id.ToString()).ToList());
        }

        public List<Response> Update(List<string> assetIds)
        {
            return CreateOrUpdate(assetIds);
        }

        public List<Response> Delete(StructAssetsDto assetsDto)
        {
            return Delete(assetsDto?.MediasItems?.Select(x => x.Id.ToString()).ToList());
        }

        public List<Response> Delete(List<string> assetIds)
        {
            if (assetIds == null || assetIds.Count <= 0)
            {
                return new List<Response>
                {
                    new Response
                    {
                        Message = "Request body is empty, or does not contain any data",
                        Status = HttpStatusCode.BadRequest,
                        Success = false
                    }
                };
            }

            assetIds = assetIds.Distinct().ToList();

            var responses = new List<Response>();
            foreach (var assetId in assetIds)
            {
                var response = _enterspeedIngestService.Delete(_entityIdentityService.GetAssetId(assetId));
                if (response != null)
                {
                    responses.Add(response);
                }
            }

            return responses;
        }

        private List<Response> CreateOrUpdate(List<string> ids)
        {
            if (ids == null || ids.Count <= 0)
            {
                return new List<Response>
                {
                    new Response
                    {
                        Message = "Request body is empty, or does not contain any data",
                        Status = HttpStatusCode.BadRequest,
                        Success = false
                    }
                };
            }

            ids = ids.Distinct().ToList();

            // Struct can only handle 5000 entities, so we need to batch them
            var assetIdBatches = ids.Batch(5000);
            var assets = new List<AssetModel>();

            foreach (var assetIdBatch in assetIdBatches)
            {
                assets.AddRange(_structPimApiClient.Assets.GetAssets(assetIdBatch.ToList()));
            }

            var responses = new List<Response>();
            foreach (var asset in assets)
            {
                var entry = new EnterspeedAssetEntity(asset, _entityIdentityService, _enterspeedPropertyService);
                var response = _enterspeedIngestService.Save(entry);
                if (response != null)
                {
                    responses.Add(response);
                }
            }

            return responses;
        }
    }
}
