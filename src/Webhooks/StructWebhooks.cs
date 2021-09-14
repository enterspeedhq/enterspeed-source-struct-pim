using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using Enterspeed.Integration.Struct.Constants;
using Enterspeed.Integration.Struct.Models;
using Enterspeed.Integration.Struct.Models.Struct;
using Enterspeed.Integration.Struct.Repository;
using Enterspeed.Integration.Struct.Services;
using Enterspeed.Source.Sdk.Api.Services;
using Enterspeed.Source.Sdk.Domain.Connection;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Struct.PIM.Api.Client;

namespace Enterspeed.Integration.Struct.Webhooks
{
    public class StructWebhooks
    {
        private readonly StructPIMApiClient _structPimApiClient;
        private readonly IEnterspeedPropertyService _enterspeedPropertyService;
        private readonly IEntityIdentityService _entityIdentityService;
        private readonly IEnterspeedIngestService _enterspeedIngestService;
        private readonly IStructLanguageRepository _languageRepository;

        public StructWebhooks(
            StructPIMApiClient structPimApiClient,
            IEnterspeedPropertyService enterspeedPropertyService,
            IEntityIdentityService entityIdentityService,
            IEnterspeedIngestService enterspeedIngestService,
            IStructLanguageRepository languageRepository)
        {
            _structPimApiClient = structPimApiClient;
            _enterspeedPropertyService = enterspeedPropertyService;
            _entityIdentityService = entityIdentityService;
            _enterspeedIngestService = enterspeedIngestService;
            _languageRepository = languageRepository;
        }

        [FunctionName("StructWebhooks")]
        public IActionResult Run([HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = null)] HttpRequestMessage req,
            ILogger log)
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

            if (eventType.Equals(WebhookConstants.GlobalListValueCreated)
                || eventType.Equals(WebhookConstants.GlobalListValueUpdated))
            {
                var requestData = GetRequestData<List<StructGlobalListValueDto>>(req);
                var responses = HandleGlobalListValueCreatedOrUpdated(requestData);
                if (!responses.TrueForAll(x => x.Success))
                {
                    return new UnprocessableEntityObjectResult(responses.Where(x => !x.Success));
                }

                return new OkObjectResult(responses);
            }

            if (eventType.Equals(WebhookConstants.GlobalListValueDeleted))
            {
                var requestData = GetRequestData<List<StructGlobalListValueDto>>(req);
                var responses = HandleGlobalListValueDeleted(requestData);
                if (!responses.TrueForAll(x => x.Success))
                {
                    return new UnprocessableEntityObjectResult(responses.Where(x => !x.Success));
                }

                return new OkObjectResult(responses);
            }

            if (eventType.Equals(WebhookConstants.MediasCreated)
            || eventType.Equals(WebhookConstants.MediasUpdated))
            {
                var requestData = GetRequestData<StructAssetsDto>(req);
                var responses = HandleAssetCreatedOrUpdated(requestData);
                if (!responses.TrueForAll(x => x.Success))
                {
                    return new UnprocessableEntityObjectResult(responses.Where(x => !x.Success));
                }

                return new OkObjectResult(responses);
            }
            
            if (eventType.Equals(WebhookConstants.MediasDeleted))
            {
                var requestData = GetRequestData<StructAssetsDto>(req);
                var responses = HandleAssetDeleted(requestData);
                if (!responses.TrueForAll(x => x.Success))
                {
                    return new UnprocessableEntityObjectResult(responses.Where(x => !x.Success));
                }

                return new OkObjectResult(responses);
            }
            
            if (eventType.Equals(WebhookConstants.ProductsCreated))
            {
                var requestData = GetRequestData<StructProductsCreatedDto>(req);
                var responses = HandleProductsCreated(requestData);
                if (!responses.TrueForAll(x => x.Success))
                {
                    return new UnprocessableEntityObjectResult(responses.Where(x => !x.Success));
                }

                return new OkObjectResult(responses);
            }
            
            if (eventType.Equals(WebhookConstants.ProductsUpdated))
            {
                var requestData = GetRequestData<StructProductsUpdatedDto>(req);
                var responses = HandleProductsUpdated(requestData);
                if (!responses.TrueForAll(x => x.Success))
                {
                    return new UnprocessableEntityObjectResult(responses.Where(x => !x.Success));
                }

                return new OkObjectResult(responses);
            }
            
            if (eventType.Equals(WebhookConstants.ProductsDeleted))
            {
                var requestData = GetRequestData<StructProductsDeletedDto>(req);
                var responses = HandleProductsDeleted(requestData);
                if (!responses.TrueForAll(x => x.Success))
                {
                    return new UnprocessableEntityObjectResult(responses.Where(x => !x.Success));
                }

                return new OkObjectResult(responses);
            }

            return new NotFoundResult();
        }

        private List<Response> HandleGlobalListValueCreatedOrUpdated(List<StructGlobalListValueDto> globalListValues)
        {
            if (globalListValues == null || globalListValues.Count <= 0)
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

            var languages = _languageRepository.GetLanguages();
            var cultureCodes = languages.Select(x => x.CultureCode).ToList();

            var responses = new List<Response>();

            foreach (var item in globalListValues)
            {
                var globalList = _structPimApiClient.GlobalLists.GetGlobalList(item.GlobalListAlias);
                var globalListValue = _structPimApiClient.GlobalLists.GetGlobalListValue(item.Uid);

                foreach (var cultureCode in cultureCodes)
                {
                    var entry = new EnterspeedGlobalListEntity(
                        globalListValue,
                        globalList,
                        cultureCode,
                        _entityIdentityService,
                        _enterspeedPropertyService);

                    var response = _enterspeedIngestService.Save(entry);
                    if (response != null)
                    {
                        responses.Add(response);
                    }
                }
            }

            return responses;
        }
        private List<Response> HandleGlobalListValueDeleted(List<StructGlobalListValueDto> globalListValues)
        {
            if (globalListValues == null || globalListValues.Count <= 0)
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
            var languages = _languageRepository.GetLanguages();
            var cultureCodes = languages.Select(x => x.CultureCode).ToList();
            var responses = new List<Response>();

            foreach (var cultureCode in cultureCodes)
            {
                foreach (var deletedValue in globalListValues)
                {
                    var response = _enterspeedIngestService.Delete(_entityIdentityService.GetId(deletedValue.Uid, cultureCode));
                    if (response != null)
                    {
                        responses.Add(response);
                    }
                }
            }

            return responses;
        }
        private List<Response> HandleAssetCreatedOrUpdated(StructAssetsDto assetsDto)
        {
            if (assetsDto?.MediasItems == null || assetsDto.MediasItems.Count <= 0)
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

            var assetIds = assetsDto.MediasItems.Select(x => x.Id.ToString()).Distinct().ToList();
            var assets = _structPimApiClient.Assets.GetAssets(assetIds);

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
        private List<Response> HandleAssetDeleted(StructAssetsDto assetsDto)
        {
            if (assetsDto?.MediasItems == null || assetsDto.MediasItems.Count <= 0)
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

            var responses = new List<Response>();
            foreach (var asset in assetsDto.MediasItems)
            {
                var response = _enterspeedIngestService.Delete(_entityIdentityService.GetAssetId(asset.Id));
                if (response != null)
                {
                    responses.Add(response);
                }
            }

            return responses;
        }

        private List<Response> HandleProductsCreated(StructProductsCreatedDto productsCreatedDto)
        {
            if (productsCreatedDto?.ProductIds == null || productsCreatedDto.ProductIds.Count <= 0)
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

            var products = _structPimApiClient.Products.GetProducts(productsCreatedDto.ProductIds);
            var languages = _languageRepository.GetLanguages();
            var cultureCodes = languages.Select(x => x.CultureCode).ToList();

            var responses = new List<Response>();
            foreach (var product in products)
            {
                foreach (var cultureCode in cultureCodes)
                {
                    var entry =
                        new EnterspeedProductEntity(product, cultureCode, _entityIdentityService, _enterspeedPropertyService);
                    var response = _enterspeedIngestService.Save(entry);

                    if (response != null)
                    {
                        responses.Add(response);
                    }
                }
            }

            return responses;
        }

        private List<Response> HandleProductsUpdated(StructProductsUpdatedDto productsUpdatedDto)
        {
            if (productsUpdatedDto?.ProductChanges == null || productsUpdatedDto.ProductChanges.Count <= 0)
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

            var productIds = productsUpdatedDto.ProductChanges.Select(x => x.Id).Distinct().ToList();
            var products = _structPimApiClient.Products.GetProducts(productIds);
            var languages = _languageRepository.GetLanguages();
            var cultureCodes = languages.Select(x => x.CultureCode).ToList();

            var responses = new List<Response>();
            foreach (var product in products)
            {
                foreach (var cultureCode in cultureCodes)
                {
                    var entry =
                        new EnterspeedProductEntity(product, cultureCode, _entityIdentityService, _enterspeedPropertyService);
                    var response = _enterspeedIngestService.Save(entry);

                    if (response != null)
                    {
                        responses.Add(response);
                    }
                }
            }

            return responses;
        }

        private List<Response> HandleProductsDeleted(StructProductsDeletedDto dto)
        {
            if (dto?.ProductIds == null || dto.ProductIds.Count <= 0)
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

            var languages = _languageRepository.GetLanguages();
            var cultureCodes = languages.Select(x => x.CultureCode).ToList();
            var responses = new List<Response>();

            foreach (var cultureCode in cultureCodes)
            {
                foreach (var productId in dto.ProductIds)
                {
                    var response = _enterspeedIngestService.Delete(_entityIdentityService.GetId(productId, cultureCode));
                    if (response != null)
                    {
                        responses.Add(response);
                    }
                }
            }

            return responses;
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
