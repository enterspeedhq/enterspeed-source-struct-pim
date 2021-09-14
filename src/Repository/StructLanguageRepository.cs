using System;
using System.Collections.Generic;
using System.Linq;
using Struct.PIM.Api.Client;
using Struct.PIM.Api.Models.Language;

namespace Enterspeed.Integration.Struct.Repository
{
    public class StructLanguageRepository : IStructLanguageRepository
    {
        private readonly StructPIMApiClient _structPimApiClient;
        private readonly Lazy<Dictionary<int, LanguageModel>> _languages;

        public StructLanguageRepository(StructPIMApiClient structPimApiClient)
        {
            _structPimApiClient = structPimApiClient;
            _languages = new Lazy<Dictionary<int, LanguageModel>>(LoadLanguages);
        }

        private Dictionary<int, LanguageModel> Languages => _languages.Value;

        public LanguageModel GetLanguage(int id)
        {
            if (Languages.ContainsKey(id))
            {
                return Languages[id];
            }

            return null;
        }

        public List<LanguageModel> GetLanguages()
        {
            return Languages.Values.ToList();
        }

        private Dictionary<int, LanguageModel> LoadLanguages()
        {
            return _structPimApiClient.Languages.GetLanguages().ToDictionary(x => x.Id);
        }
    }
}
