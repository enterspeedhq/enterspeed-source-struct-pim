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
using Struct.PIM.Api.Models.Variant;

namespace Enterspeed.Integration.Struct.Services.IngestServices
{
    public class VariantEnterspeedIngestService : IVariantIngestService
    {
        private readonly StructPIMApiClient _structPimApiClient;
        private readonly IStructLanguageRepository _languageRepository;
        private readonly IEntityIdentityService _entityIdentityService;
        private readonly IEnterspeedPropertyService _enterspeedPropertyService;
        private readonly IEnterspeedIngestService _enterspeedIngestService;
        private readonly IStructAttributeRepository _structAttributeRepository;

        public VariantEnterspeedIngestService(
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

        public List<Response> Create(StructVariantsCreatedDto dto)
        {
            return Create(dto?.VariantIds);
        }

        public List<Response> Create(List<int> variantIds)
        {
            return CreateOrUpdate(variantIds);
        }

        public List<Response> Update(StructVariantsUpdatedDto dto)
        {
            return Update(dto?.VariantIds);
        }

        public List<Response> Update(List<int> variantIds)
        {
            return CreateOrUpdate(variantIds);
        }

        public List<Response> Delete(StructVariantsDeletedDto dto)
        {
            return Delete(dto?.VariantIds);
        }

        public List<Response> Delete(List<int> variantIds)
        {
            if (variantIds == null || variantIds.Count <= 0)
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

            variantIds = variantIds.Distinct().ToList();

            var languages = _languageRepository.GetLanguages();
            var cultureCodes = languages.Select(x => x.CultureCode).ToList();
            var responses = new List<Response>();

            foreach (var cultureCode in cultureCodes)
            {
                foreach (var variantId in variantIds)
                {
                    var response = _enterspeedIngestService.Delete(_entityIdentityService.GetVariantId(variantId, cultureCode));
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
            var variantIdBatches = ids.Batch(5000);
            var variants = new List<VariantModel>();
            var variantsAttributeValues = new List<VariantAttributeValuesModel>();

            foreach (var variantIdBatch in variantIdBatches)
            {
                var variantIds = variantIdBatch.ToList();
                var variantModels = _structPimApiClient.Variants.GetVariants(variantIds);
                variants.AddRange(variantModels);

                var attributeValuesRequestModel = new VariantValuesRequestModel
                {
                    VariantIds = variantIds,
                    GlobalListValueReferencesOnly = true
                };

                variantsAttributeValues.AddRange(_structPimApiClient.Variants.GetVariantAttributeValues(attributeValuesRequestModel));
            }

            var variantsAttributeValuesDict = variantsAttributeValues.ToDictionary(x => x.VariantId, x => x.Values);

            var allAttributes = _structAttributeRepository.GetAllAttributes().ToDictionary(x => x.Alias);

            var languages = _languageRepository.GetLanguages();
            var cultureCodes = languages.Select(x => x.CultureCode).ToList();

            var responses = new List<Response>();
            foreach (var variant in variants)
            {
                var variantAttributeValues =
                    variantsAttributeValuesDict.ContainsKey(variant.Id) ? variantsAttributeValuesDict[variant.Id] : null;

                if (variantAttributeValues == null)
                {
                    continue;
                }

                var variantAttributes = new Dictionary<Attribute, dynamic>();

                foreach (var variantAttributeValue in variantAttributeValues)
                {
                    if (!allAttributes.TryGetValue(variantAttributeValue.Key, out var attribute))
                    {
                        continue;
                    }

                    variantAttributes.Add(attribute, variantAttributeValue.Value);
                }

                foreach (var cultureCode in cultureCodes)
                {
                    var entry =
                        new EnterspeedVariantEntity(variant, variantAttributes, cultureCode, _entityIdentityService, _enterspeedPropertyService);
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
