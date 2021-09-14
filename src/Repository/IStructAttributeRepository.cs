using System.Collections.Generic;
using Struct.PIM.Api.Models.Attribute;

namespace Enterspeed.Integration.Struct.Repository
{
    public interface IStructAttributeRepository
    {
        List<Attribute> GetAllAttributes();
    }
}
