using System.Collections.Generic;
using Enterspeed.Integration.Struct.Models.Struct;
using Enterspeed.Source.Sdk.Domain.Connection;

namespace Enterspeed.Integration.Struct.Services.IngestServices
{
    public interface IVariantIngestService
    {
        List<Response> Create(StructVariantsCreatedDto dto);
        List<Response> Create(List<int> variantIds);
        List<Response> Update(StructVariantsUpdatedDto dto);
        List<Response> Update(List<int> variantIds);
        List<Response> Delete(StructVariantsDeletedDto dto);
        List<Response> Delete(List<int> variantIds);
    }
}
