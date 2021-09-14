using System.Collections.Generic;
using Struct.PIM.Api.Models.Language;

namespace Enterspeed.Integration.Struct.Repository
{
    public interface IStructLanguageRepository
    {
        LanguageModel GetLanguage(int id);
        List<LanguageModel> GetLanguages();
    }
}
