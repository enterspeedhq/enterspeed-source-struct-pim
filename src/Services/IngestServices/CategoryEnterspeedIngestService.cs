using System.Collections.Generic;
using System.Linq;
using System.Net;
using Enterspeed.Integration.Struct.Models;
using Enterspeed.Integration.Struct.Models.Struct;
using Enterspeed.Integration.Struct.Repository;
using Enterspeed.Source.Sdk.Api.Services;
using Enterspeed.Source.Sdk.Domain.Connection;
using MoreLinq;
using Struct.PIM.Api.Client;
using Struct.PIM.Api.Models.Attribute;
using Struct.PIM.Api.Models.Catalogue;
using Struct.PIM.Api.Models.Product;

namespace Enterspeed.Integration.Struct.Services.IngestServices
{
    public class CategoryEnterspeedIngestService : ICategoryIngestService
    {
        private readonly StructPIMApiClient _structPimApiClient;
        private readonly IStructLanguageRepository _languageRepository;
        private readonly IEntityIdentityService _entityIdentityService;
        private readonly IEnterspeedPropertyService _enterspeedPropertyService;
        private readonly IEnterspeedIngestService _enterspeedIngestService;
        private readonly IStructAttributeRepository _structAttributeRepository;

        public CategoryEnterspeedIngestService(
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

        public List<Response> Create(StructCategoriesCreatedDto dto)
        {
            return Create(dto?.CategoryIds);
        }

        public List<Response> Create(List<int> categoryIds)
        {
            return CreateOrUpdate(categoryIds);
        }

        public List<Response> Update(StructCategoriesUpdatedDto dto)
        {
            return Update(dto?.CategoryIds);
        }

        public List<Response> Update(List<int> categoryIds)
        {
            return CreateOrUpdate(categoryIds);
        }

        public List<Response> Delete(StructCategoriesDeletedDto dto)
        {
            return Delete(dto?.CategoryIds);
        }

        public List<Response> Delete(List<int> categoryIds)
        {
            if (categoryIds == null || categoryIds.Count <= 0)
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

            categoryIds = categoryIds.Distinct().ToList();

            var languages = _languageRepository.GetLanguages();
            var cultureCodes = languages.Select(x => x.CultureCode).ToList();
            var responses = new List<Response>();

            foreach (var cultureCode in cultureCodes)
            {
                foreach (var categoryId in categoryIds)
                {
                    var response = _enterspeedIngestService.Delete(_entityIdentityService.GetCategoryId(categoryId, cultureCode));
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

            var categoryIdBatches = ids.Batch(5000);
            var categories = new List<CategoryModel>();
            var categoriesAttributeValues = new List<CategoryAttributeValuesModel>();


            foreach (var categoryIdBatch in categoryIdBatches)
            {
                var categoryIds = categoryIdBatch.ToList();

                var categoryModels = _structPimApiClient.Catalogues.GetCategories(categoryIds);
                categories.AddRange(categoryModels);

                var attributeValuesRequestModel = new CategoryValueRequestModel()
                {
                    CategoryIds = categoryIds,
                    GlobalListValueReferencesOnly = true
                };

                categoriesAttributeValues.AddRange(_structPimApiClient.Catalogues.GetCategoryAttributeValues(attributeValuesRequestModel));
            }

            var categoriesAttributeValuesDict = categoriesAttributeValues.ToDictionary(x => x.CategoryId, x => x.Values);

            var languages = _languageRepository.GetLanguages();
            var cultureCodes = languages.Select(x => x.CultureCode).ToList();
            var allAttributes = _structAttributeRepository.GetAllAttributes().ToDictionary(x => x.Alias);

            var responses = new List<Response>();
            foreach (var category in categories)
            {
                var categoryAttributeValues =
                    categoriesAttributeValuesDict.ContainsKey(category.Id) ? categoriesAttributeValuesDict[category.Id] : null;

                if (categoryAttributeValues == null)
                {
                    continue;
                }

                var categoryAttributes = new Dictionary<Attribute, dynamic>();

                foreach (var categoryAttributeValue in categoryAttributeValues)
                {
                    if (!allAttributes.TryGetValue(categoryAttributeValue.Key, out var attribute))
                    {
                        continue;
                    }

                    categoryAttributes.Add(attribute, categoryAttributeValue.Value);
                }

                foreach (var cultureCode in cultureCodes)
                {
                    var entry =
                        new EnterspeedCategoryEntity(category, categoryAttributes, cultureCode, _entityIdentityService, _enterspeedPropertyService);
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
