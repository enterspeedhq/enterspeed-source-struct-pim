using System.Collections.Generic;
using System.Linq;
using System.Net;
using Enterspeed.Integration.Struct.Models;
using Enterspeed.Integration.Struct.Models.Struct;
using Enterspeed.Integration.Struct.Repository;
using Enterspeed.Source.Sdk.Api.Services;
using Enterspeed.Source.Sdk.Domain.Connection;
using MoreLinq.Extensions;
using Struct.PIM.Api.Client;
using Struct.PIM.Api.Models.Attribute;
using Struct.PIM.Api.Models.Product;

namespace Enterspeed.Integration.Struct.Services.IngestServices
{
    public class ProductEnterspeedIngestService : IProductIngestService
    {
        private readonly StructPIMApiClient _structPimApiClient;
        private readonly IStructLanguageRepository _languageRepository;
        private readonly IEntityIdentityService _entityIdentityService;
        private readonly IEnterspeedPropertyService _enterspeedPropertyService;
        private readonly IEnterspeedIngestService _enterspeedIngestService;
        private readonly IStructAttributeRepository _structAttributeRepository;

        public ProductEnterspeedIngestService(
            StructPIMApiClient structPimApiClient,
            IStructLanguageRepository languageRepository,
            IEntityIdentityService entityIdentityService,
            IEnterspeedPropertyService enterspeedPropertyService,
            IEnterspeedIngestService enterspeedIngestService,
            IStructAttributeRepository structAttributeRepository)
        {
            _structPimApiClient = structPimApiClient;
            _languageRepository = languageRepository;
            _entityIdentityService = entityIdentityService;
            _enterspeedPropertyService = enterspeedPropertyService;
            _enterspeedIngestService = enterspeedIngestService;
            _structAttributeRepository = structAttributeRepository;
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

        public List<Response> Delete(List<int> ids)
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

            var languages = _languageRepository.GetLanguages();
            var cultureCodes = languages.Select(x => x.CultureCode).ToList();
            var responses = new List<Response>();

            foreach (var cultureCode in cultureCodes)
            {
                foreach (var productId in ids)
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

        private List<Response> CreateOrUpdate(List<int> ids)
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
            var productIdBatches = ids.Batch(5000);
            var products = new List<ProductModel>();
            var productsAttributeValues = new List<ProductAttributeValuesModel>();

            foreach (var productIdBatch in productIdBatches)
            {
                var productIds = productIdBatch.ToList();
                var productModels = _structPimApiClient.Products.GetProducts(productIds);
                products.AddRange(productModels);

                var attributeValuesRequestModel = new ProductValuesRequestModel()
                {
                    ProductIds = productIds,
                    GlobalListValueReferencesOnly = true
                };

                productsAttributeValues.AddRange(_structPimApiClient.Products.GetProductAttributeValues(attributeValuesRequestModel));
            }

            var productsAttributeValuesDict = productsAttributeValues.ToDictionary(x => x.ProductId, x => x.Values);

            var languages = _languageRepository.GetLanguages();
            var cultureCodes = languages.Select(x => x.CultureCode).ToList();
            var allAttributes = _structAttributeRepository.GetAllAttributes().ToDictionary(x => x.Alias);

            var responses = new List<Response>();
            foreach (var product in products)
            {
                var productAttributeValues =
                    productsAttributeValuesDict.ContainsKey(product.Id) ? productsAttributeValuesDict[product.Id] : null;

                if (productAttributeValues == null)
                {
                    continue;
                }

                var variantAttributes = new Dictionary<Attribute, dynamic>();

                foreach (var productAttributeValue in productAttributeValues)
                {
                    if (!allAttributes.TryGetValue(productAttributeValue.Key, out var attribute))
                    {
                        continue;
                    }

                    variantAttributes.Add(attribute, productAttributeValue.Value);
                }

                foreach (var cultureCode in cultureCodes)
                {
                    var entry =
                        new EnterspeedProductEntity(product, variantAttributes, cultureCode, _entityIdentityService, _enterspeedPropertyService);
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
