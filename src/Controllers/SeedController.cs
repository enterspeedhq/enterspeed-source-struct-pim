using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using Enterspeed.Integration.Struct.Models.Struct;
using Enterspeed.Integration.Struct.Services.IngestServices;
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

        public SeedController(
            StructPIMApiClient structPimApiClient,
            IGlobalListIngestService globalListIngestService)
        {
            _structPimApiClient = structPimApiClient;
            _globalListIngestService = globalListIngestService;
        }

        [FunctionName("SeedAll")]
        public IActionResult Run([HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = null)] HttpRequestMessage req, ILogger log)
        {
            var allGlobalListValues = GetAllGlobalListValues();
            var responses = _globalListIngestService.Update(allGlobalListValues);

            if (!responses.TrueForAll(x => x.Success))
            {
                return new UnprocessableEntityObjectResult(responses.Where(x => !x.Success));
            }

            return new OkObjectResult(responses);
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
                    allGlobalListValues.AddRange(globalListValuesResult.GlobalListValues.Select(x => new StructGlobalListValueDto
                    {
                        Uid = x.Uid,
                        GlobalListAlias = globalList.Attribute.Alias,
                        GlobalListId = globalList.Id
                    }));

                    while (globalListValuesResult.Remaining > 0)
                    {
                        globalListValuesResult = _structPimApiClient.GlobalLists.GetGlobalListValues(globalList.Uid, 1000, globalListValuesResult.LastId);
                        allGlobalListValues.AddRange(globalListValuesResult.GlobalListValues.Select(x => new StructGlobalListValueDto
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
