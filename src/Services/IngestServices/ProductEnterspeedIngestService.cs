using System.Collections.Generic;
using System.Linq;
using System.Net;
using Enterspeed.Integration.Struct.Models;
using Enterspeed.Integration.Struct.Models.Struct;
using Enterspeed.Integration.Struct.Repository;
using Enterspeed.Source.Sdk.Api.Services;
using Enterspeed.Source.Sdk.Domain.Connection;
using Struct.PIM.Api.Client;

namespace Enterspeed.Integration.Struct.Services.IngestServices
{
    public class ProductEnterspeedIngestService : IProductIngestService
    {
        private readonly StructPIMApiClient _structPimApiClient;
        private readonly IStructLanguageRepository _languageRepository;
        private readonly IEntityIdentityService _entityIdentityService;
        private readonly IEnterspeedPropertyService _enterspeedPropertyService;
        private readonly IEnterspeedIngestService _enterspeedIngestService;

        public ProductEnterspeedIngestService(
            StructPIMApiClient structPimApiClient,
            IStructLanguageRepository languageRepository,
            IEntityIdentityService entityIdentityService,
            IEnterspeedPropertyService enterspeedPropertyService,
            IEnterspeedIngestService enterspeedIngestService)
        {
            _structPimApiClient = structPimApiClient;
            _languageRepository = languageRepository;
            _entityIdentityService = entityIdentityService;
            _enterspeedPropertyService = enterspeedPropertyService;
            _enterspeedIngestService = enterspeedIngestService;
        }

        public List<Response> Create(StructProductsCreatedDto productsCreatedDto)
        {
            return Create(productsCreatedDto?.ProductIds);
        }

        public List<Response> Create(List<int> productIds)
        {
            return CreateOrUpdate(productIds);
        }

        public List<Response> Update(StructProductsUpdatedDto productsUpdatedDto)
        {
            return Update(productsUpdatedDto?.ProductChanges?.Select(x => x.Id).ToList());
        }

        public List<Response> Update(List<int> productIds)
        {
            return CreateOrUpdate(productIds);
        }

        public List<Response> Delete(StructProductsDeletedDto productsDeletedDto)
        {
            return Delete(productsDeletedDto?.ProductIds);
        }

        public List<Response> Delete(List<int> productIds)
        {
            if (productIds == null || productIds.Count <= 0)
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

            productIds = productIds.Distinct().ToList();

            var languages = _languageRepository.GetLanguages();
            var cultureCodes = languages.Select(x => x.CultureCode).ToList();
            var responses = new List<Response>();

            foreach (var cultureCode in cultureCodes)
            {
                foreach (var productId in productIds)
                {
                    var response = _enterspeedIngestService.Delete(_entityIdentityService.GetProductId(productId, cultureCode));
                    if (response != null)
                    {
                        responses.Add(response);
                    }
                }
            }

            return responses;
        }

        private List<Response> CreateOrUpdate(List<int> productIds)
        {
            if (productIds == null || productIds.Count <= 0)
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

            productIds = productIds.Distinct().ToList();

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
    }
}
