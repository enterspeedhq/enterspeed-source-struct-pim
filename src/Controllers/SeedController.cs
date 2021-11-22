using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using Enterspeed.Integration.Struct.Models.Struct;
using Enterspeed.Integration.Struct.Services.IngestServices;
using Enterspeed.Source.Sdk.Domain.Connection;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Extensions.Logging;
using Struct.PIM.Api.Client;

namespace Enterspeed.Integration.Struct.Controllers
{
    public class SeedController
    {
        private readonly StructPIMApiClient _structPimApiClient;
        private readonly IGlobalListIngestService _globalListIngestService;
        private readonly IAssetIngestService _assetIngestService;
        private readonly ICategoryIngestService _categoryIngestService;
        private readonly IVariantIngestService _variantIngestService;
        private readonly IProductIngestService _productIngestService;

        public SeedController(
            StructPIMApiClient structPimApiClient,
            IGlobalListIngestService globalListIngestService,
            IAssetIngestService assetIngestService,
            ICategoryIngestService categoryIngestService,
            IVariantIngestService variantIngestService,
            IProductIngestService productIngestService)
        {
            _structPimApiClient = structPimApiClient;
            _globalListIngestService = globalListIngestService;
            _assetIngestService = assetIngestService;
            _categoryIngestService = categoryIngestService;
            _variantIngestService = variantIngestService;
            _productIngestService = productIngestService;
        }

        [FunctionName("SeedAll")]
        public IActionResult SeedAll([HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = null)] HttpRequestMessage req, ILogger log)
        {
            var responses = new List<Response>();

            // Seed Global list values
            var globalListSeedResponses = SeedAllGlobalLists();
            responses.AddRange(globalListSeedResponses);

            // Seed assets
            var assetSeedResponses = SeedAllAssets();
            responses.AddRange(assetSeedResponses);

            // Seed categories
            var categorySeedResponses = SeedAllCategories();
            responses.AddRange(categorySeedResponses);

            // Seed variants
            var variantSeedResponses = SeedAllVariants();
            responses.AddRange(variantSeedResponses);

            // Seed products
            var productSeedResponses = SeedAllProducts();
            responses.AddRange(productSeedResponses);

            if (!responses.TrueForAll(x => x.Success))
            {
                return new UnprocessableEntityObjectResult(responses.Where(x => !x.Success));
            }

            return new OkObjectResult(responses);
        }

        [FunctionName("SeedGlobalLists")]
        public IActionResult SeedGlobalLists([HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = null)] HttpRequestMessage req, ILogger log)
        {
            var responses = SeedAllGlobalLists();

            if (!responses.TrueForAll(x => x.Success))
            {
                return new UnprocessableEntityObjectResult(responses.Where(x => !x.Success));
            }

            return new OkObjectResult(responses);
        }

        [FunctionName("SeedAssets")]
        public IActionResult SeedAssets([HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = null)] HttpRequestMessage req, ILogger log)
        {
            var responses = SeedAllAssets();

            if (!responses.TrueForAll(x => x.Success))
            {
                return new UnprocessableEntityObjectResult(responses.Where(x => !x.Success));
            }

            return new OkObjectResult(responses);
        }

        [FunctionName("SeedCategories")]
        public IActionResult SeedCategories([HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = null)] HttpRequestMessage req, ILogger log)
        {
            var responses = SeedAllCategories();

            if (!responses.TrueForAll(x => x.Success))
            {
                return new UnprocessableEntityObjectResult(responses.Where(x => !x.Success));
            }

            return new OkObjectResult(responses);
        }

        [FunctionName("SeedVariants")]
        public IActionResult SeedVariants([HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = null)] HttpRequestMessage req, ILogger log)
        {
            var responses = SeedAllVariants();

            if (!responses.TrueForAll(x => x.Success))
            {
                return new UnprocessableEntityObjectResult(responses.Where(x => !x.Success));
            }

            return new OkObjectResult(responses);
        }

        [FunctionName("SeedProducts")]
        public IActionResult SeedProducts([HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = null)] HttpRequestMessage req, ILogger log)
        {
            var responses = SeedAllProducts();

            if (!responses.TrueForAll(x => x.Success))
            {
                return new UnprocessableEntityObjectResult(responses.Where(x => !x.Success));
            }

            return new OkObjectResult(responses);
        }

        private List<Response> SeedAllGlobalLists()
        {
            var allGlobalListValues = GetAllGlobalListValues();
            return _globalListIngestService.Update(allGlobalListValues);
        }

        private List<Response> SeedAllAssets()
        {
            var allAssetIds = _structPimApiClient.Assets.GetAssetIds();
            return _assetIngestService.Update(allAssetIds);
        }

        private List<Response> SeedAllCategories()
        {
            var allCategoryIds = _structPimApiClient.Catalogues.GetCategoryIds();
            return _categoryIngestService.Update(allCategoryIds);
        }

        private List<Response> SeedAllVariants()
        {
            var allVariantIds = _structPimApiClient.Variants.GetVariantIds();
            return _variantIngestService.Update(allVariantIds);
        }

        private List<Response> SeedAllProducts()
        {
            var allProductIds = _structPimApiClient.Products.GetProductIds();
            return _productIngestService.Update(allProductIds);
        }

        private List<StructGlobalListValueDto> GetAllGlobalListValues()
        {
            var allGlobalLists = _structPimApiClient.GlobalLists.GetGlobalLists();

            var allGlobalListValues = new List<StructGlobalListValueDto>();
            foreach (var globalList in allGlobalLists)
            {
                var globalListValuesResult = _structPimApiClient.GlobalLists.GetGlobalListValues(globalList.Uid);
                if (globalListValuesResult != null)
                {
                    allGlobalListValues.AddRange(globalListValuesResult.GlobalListValues.Select(x =>
                        new StructGlobalListValueDto
                        {
                            Uid = x.Uid,
                            GlobalListAlias = globalList.Attribute.Alias,
                            GlobalListId = globalList.Id
                        }));

                    while (globalListValuesResult.Remaining > 0)
                    {
                        globalListValuesResult =
                            _structPimApiClient.GlobalLists.GetGlobalListValues(globalList.Uid, 1000,
                                globalListValuesResult.LastId);
                        allGlobalListValues.AddRange(globalListValuesResult.GlobalListValues.Select(x =>
                            new StructGlobalListValueDto
                            {
                                Uid = x.Uid,
                                GlobalListAlias = globalList.Attribute.Alias,
                                GlobalListId = globalList.Id
                            }));
                    }
                }
            }

            return allGlobalListValues;
        }
    }
}
