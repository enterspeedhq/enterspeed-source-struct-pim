using System.Collections.Generic;
using Enterspeed.Integration.Struct.Models.Struct;
using Enterspeed.Source.Sdk.Domain.Connection;

namespace Enterspeed.Integration.Struct.Services.IngestServices
{
    public interface IGlobalListIngestService
    {
        List<Response> Create(List<StructGlobalListValueDto> globalListValues);
        List<Response> Update(List<StructGlobalListValueDto> globalListValues);
        List<Response> Delete(List<StructGlobalListValueDto> globalListValues);
    }
}
