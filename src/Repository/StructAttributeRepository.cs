using System.Collections.Generic;
using Struct.PIM.Api.Client;
using Struct.PIM.Api.Models.Attribute;

namespace Enterspeed.Integration.Struct.Repository
{
    public class StructAttributeRepository : IStructAttributeRepository
    {
        private readonly StructPIMApiClient _structPimApiClient;
        private List<Attribute> _allAttributes;
        public StructAttributeRepository(
            StructPIMApiClient structPimApiClient)
        {
            _structPimApiClient = structPimApiClient;
        }

        public List<Attribute> GetAllAttributes()
        {
            if (_allAttributes == null || _allAttributes.Count <= 0)
            {
                _allAttributes = _structPimApiClient.Attributes.GetAttributes();
            }

            return _allAttributes;
        }
    }
}
