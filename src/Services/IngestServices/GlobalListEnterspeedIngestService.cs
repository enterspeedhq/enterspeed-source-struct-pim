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
    public class GlobalListEnterspeedIngestService : IGlobalListIngestService
    {
        private readonly StructPIMApiClient _structPimApiClient;
        private readonly IStructLanguageRepository _languageRepository;
        private readonly IEntityIdentityService _entityIdentityService;
        private readonly IEnterspeedPropertyService _enterspeedPropertyService;
        private readonly IEnterspeedIngestService _enterspeedIngestService;

        public GlobalListEnterspeedIngestService(
            IStructLanguageRepository languageRepository,
            StructPIMApiClient structPimApiClient,
            IEntityIdentityService entityIdentityService,
            IEnterspeedPropertyService enterspeedPropertyService,
            IEnterspeedIngestService enterspeedIngestService)
        {
            _languageRepository = languageRepository;
            _structPimApiClient = structPimApiClient;
            _entityIdentityService = entityIdentityService;
            _enterspeedPropertyService = enterspeedPropertyService;
            _enterspeedIngestService = enterspeedIngestService;
        }

        public List<Response> Create(List<StructGlobalListValueDto> globalListValues)
        {
            return CreateOrUpdate(globalListValues);
        }

        public List<Response> Update(List<StructGlobalListValueDto> globalListValues)
        {
            return CreateOrUpdate(globalListValues);
        }

        public List<Response> Delete(List<StructGlobalListValueDto> globalListValues)
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

        private List<Response> CreateOrUpdate(List<StructGlobalListValueDto> globalListValues)
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

            var globalLists = _structPimApiClient
                .GlobalLists
                .GetGlobalLists(globalListValues.Select(x => x.GlobalListAlias).Distinct().ToList())
                .ToDictionary(x => x.Attribute.Alias);

            var values = _structPimApiClient
                .GlobalLists
                .GetGlobalListValues(globalListValues.Select(x => x.Uid).Distinct().ToList())
                .ToDictionary(x => x.Uid);

            var responses = new List<Response>();

            foreach (var item in globalListValues)
            {
                if (!globalLists.TryGetValue(item.GlobalListAlias, out var globalList))
                {
                    continue;
                }

                if (!values.TryGetValue(item.Uid, out var globalListValue))
                {
                    continue;
                }

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
    }
}
