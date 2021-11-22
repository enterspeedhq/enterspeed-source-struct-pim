using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using Enterspeed.Integration.Struct.Constants;
using Enterspeed.Integration.Struct.Models.Struct;
using Enterspeed.Integration.Struct.Services.IngestServices;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace Enterspeed.Integration.Struct.Controllers
{
    public class StructWebhooks
    {
        private readonly IGlobalListIngestService _globalListIngestService;
        private readonly IAssetIngestService _assetIngestService;
        private readonly IProductIngestService _productIngestService;
        private readonly IVariantIngestService _variantIngestService;
        private readonly ICategoryIngestService _categoryIngestService;

        public StructWebhooks(
            IGlobalListIngestService globalListIngestService,
            IAssetIngestService assetIngestService,
            IProductIngestService productIngestService,
            IVariantIngestService variantIngestService,
            ICategoryIngestService categoryIngestService)
        {
            _globalListIngestService = globalListIngestService;
            _assetIngestService = assetIngestService;
            _productIngestService = productIngestService;
            _variantIngestService = variantIngestService;
            _categoryIngestService = categoryIngestService;
        }

        [FunctionName("StructWebhooks")]
        public IActionResult Handle([HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = null)] HttpRequestMessage req, ILogger log)
        {
            if (!req.Headers.TryGetValues(WebhookConstants.EventHeaderKey, out var eventTypeValues))
            {
                return new BadRequestResult();
            }

            var eventType = eventTypeValues.FirstOrDefault();
            if (string.IsNullOrWhiteSpace(eventType))
            {
                return new BadRequestResult();
            }

            if (eventType.Equals(WebhookConstants.GlobalListValueCreated))
            {
                var requestData = GetRequestData<List<StructGlobalListValueDto>>(req);
                var responses = _globalListIngestService.Create(requestData);
                if (!responses.TrueForAll(x => x.Success))
                {
                    return new UnprocessableEntityObjectResult(responses.Where(x => !x.Success));
                }

                return new OkObjectResult(responses);
            }

            if (eventType.Equals(WebhookConstants.GlobalListValueUpdated))
            {
                var requestData = GetRequestData<List<StructGlobalListValueDto>>(req);
                var responses = _globalListIngestService.Update(requestData);
                if (!responses.TrueForAll(x => x.Success))
                {
                    return new UnprocessableEntityObjectResult(responses.Where(x => !x.Success));
                }

                return new OkObjectResult(responses);
            }

            if (eventType.Equals(WebhookConstants.GlobalListValueDeleted))
            {
                var requestData = GetRequestData<List<StructGlobalListValueDto>>(req);
                var responses = _globalListIngestService.Delete(requestData);
                if (!responses.TrueForAll(x => x.Success))
                {
                    return new UnprocessableEntityObjectResult(responses.Where(x => !x.Success));
                }

                return new OkObjectResult(responses);
            }

            if (eventType.Equals(WebhookConstants.MediasCreated))
            {
                var requestData = GetRequestData<StructAssetsDto>(req);
                var responses = _assetIngestService.Create(requestData);
                if (!responses.TrueForAll(x => x.Success))
                {
                    return new UnprocessableEntityObjectResult(responses.Where(x => !x.Success));
                }

                return new OkObjectResult(responses);
            }

            if (eventType.Equals(WebhookConstants.MediasUpdated))
            {
                var requestData = GetRequestData<StructAssetsDto>(req);
                var responses = _assetIngestService.Update(requestData);
                if (!responses.TrueForAll(x => x.Success))
                {
                    return new UnprocessableEntityObjectResult(responses.Where(x => !x.Success));
                }

                return new OkObjectResult(responses);
            }

            if (eventType.Equals(WebhookConstants.MediasDeleted))
            {
                var requestData = GetRequestData<StructAssetsDto>(req);
                var responses = _assetIngestService.Delete(requestData);
                if (!responses.TrueForAll(x => x.Success))
                {
                    return new UnprocessableEntityObjectResult(responses.Where(x => !x.Success));
                }

                return new OkObjectResult(responses);
            }

            if (eventType.Equals(WebhookConstants.ProductsCreated))
            {
                var requestData = GetRequestData<StructProductsCreatedDto>(req);
                var responses = _productIngestService.Create(requestData);
                if (!responses.TrueForAll(x => x.Success))
                {
                    return new UnprocessableEntityObjectResult(responses.Where(x => !x.Success));
                }

                return new OkObjectResult(responses);
            }

            if (eventType.Equals(WebhookConstants.ProductsUpdated))
            {
                var requestData = GetRequestData<StructProductsUpdatedDto>(req);
                var responses = _productIngestService.Update(requestData);
                if (!responses.TrueForAll(x => x.Success))
                {
                    return new UnprocessableEntityObjectResult(responses.Where(x => !x.Success));
                }

                return new OkObjectResult(responses);
            }

            if (eventType.Equals(WebhookConstants.ProductsDeleted))
            {
                var requestData = GetRequestData<StructProductsDeletedDto>(req);
                var responses = _productIngestService.Delete(requestData);
                if (!responses.TrueForAll(x => x.Success))
                {
                    return new UnprocessableEntityObjectResult(responses.Where(x => !x.Success));
                }

                return new OkObjectResult(responses);
            }

            if (eventType.Equals(WebhookConstants.VariantsCreated))
            {
                var requestData = GetRequestData<StructVariantsCreatedDto>(req);
                var responses = _variantIngestService.Create(requestData);
                if (!responses.TrueForAll(x => x.Success))
                {
                    return new UnprocessableEntityObjectResult(responses.Where(x => !x.Success));
                }

                return new OkObjectResult(responses);
            }

            if (eventType.Equals(WebhookConstants.VariantsUpdated))
            {
                var requestData = GetRequestData<StructVariantsUpdatedDto>(req);
                var responses = _variantIngestService.Update(requestData);
                if (!responses.TrueForAll(x => x.Success))
                {
                    return new UnprocessableEntityObjectResult(responses.Where(x => !x.Success));
                }

                return new OkObjectResult(responses);
            }

            if (eventType.Equals(WebhookConstants.VariantsDeleted))
            {
                var requestData = GetRequestData<StructVariantsDeletedDto>(req);
                var responses = _variantIngestService.Delete(requestData);
                if (!responses.TrueForAll(x => x.Success))
                {
                    return new UnprocessableEntityObjectResult(responses.Where(x => !x.Success));
                }

                return new OkObjectResult(responses);
            }

            if (eventType.Equals(WebhookConstants.CategoriesCreated))
            {
                var requestData = GetRequestData<StructCategoriesCreatedDto>(req);
                var responses = _categoryIngestService.Create(requestData);
                if (!responses.TrueForAll(x => x.Success))
                {
                    return new UnprocessableEntityObjectResult(responses.Where(x => !x.Success));
                }

                return new OkObjectResult(responses);
            }

            if (eventType.Equals(WebhookConstants.CategoriesUpdated))
            {
                var requestData = GetRequestData<StructCategoriesUpdatedDto>(req);
                var responses = _categoryIngestService.Update(requestData);
                if (!responses.TrueForAll(x => x.Success))
                {
                    return new UnprocessableEntityObjectResult(responses.Where(x => !x.Success));
                }

                return new OkObjectResult(responses);
            }

            if (eventType.Equals(WebhookConstants.CategoriesDeleted))
            {
                var requestData = GetRequestData<StructCategoriesDeletedDto>(req);
                var responses = _categoryIngestService.Delete(requestData);
                if (!responses.TrueForAll(x => x.Success))
                {
                    return new UnprocessableEntityObjectResult(responses.Where(x => !x.Success));
                }

                return new OkObjectResult(responses);
            }

            return new NotFoundResult();
        }

        private T GetRequestData<T>(HttpRequestMessage request)
        {
            var requstBodyJson = request.Content.ReadAsStringAsync().Result;
            if (string.IsNullOrWhiteSpace(requstBodyJson))
            {
                return default;
            }

            return JsonConvert.DeserializeObject<T>(requstBodyJson);
        }
    }
}
