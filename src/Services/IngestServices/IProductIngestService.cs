using System.Collections.Generic;
using Enterspeed.Integration.Struct.Models.Struct;
using Enterspeed.Source.Sdk.Domain.Connection;

namespace Enterspeed.Integration.Struct.Services.IngestServices
{
    public interface IProductIngestService
    {
        List<Response> Create(StructProductsCreatedDto productsCreatedDto);
        List<Response> Create(List<int> productIds);
        List<Response> Update(StructProductsUpdatedDto productsUpdatedDto);
        List<Response> Update(List<int> productIds);
        List<Response> Delete(StructProductsDeletedDto productsDeletedDto);
        List<Response> Delete(List<int> ids);
    }
}
