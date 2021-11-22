using System.Collections.Generic;
using Enterspeed.Integration.Struct.Models.Struct;
using Enterspeed.Source.Sdk.Domain.Connection;

namespace Enterspeed.Integration.Struct.Services.IngestServices
{
    public interface ICategoryIngestService
    {
        List<Response> Create(StructCategoriesCreatedDto dto);
        List<Response> Create(List<int> categoryIds);
        List<Response> Update(StructCategoriesUpdatedDto dto);
        List<Response> Update(List<int> categoryIds);
        List<Response> Delete(StructCategoriesDeletedDto dto);
        List<Response> Delete(List<int> categoryIds);
    }
}
