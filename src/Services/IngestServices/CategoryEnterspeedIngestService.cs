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
    public class CategoryEnterspeedIngestService : ICategoryIngestService
    {
        private readonly StructPIMApiClient _structPimApiClient;
        private readonly IStructLanguageRepository _languageRepository;
        private readonly IEntityIdentityService _entityIdentityService;
        private readonly IEnterspeedPropertyService _enterspeedPropertyService;
        private readonly IEnterspeedIngestService _enterspeedIngestService;

        public CategoryEnterspeedIngestService(
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

        private List<Response> CreateOrUpdate(List<int> categoryIds)
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

            var categories = _structPimApiClient.Catalogues.GetCategories(categoryIds);
            var languages = _languageRepository.GetLanguages();
            var cultureCodes = languages.Select(x => x.CultureCode).ToList();

            var responses = new List<Response>();
            foreach (var category in categories)
            {
                foreach (var cultureCode in cultureCodes)
                {
                    var entry =
                        new EnterspeedCategoryEntity(category, cultureCode, _entityIdentityService, _enterspeedPropertyService);
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
