using System.Collections.Generic;
using System.Linq;
using Enterspeed.Integration.Struct.Repository;
using Newtonsoft.Json.Linq;
using Struct.PIM.Api.Models.Attribute;
using Struct.PIM.Api.Models.Shared;

namespace Enterspeed.Integration.Struct.Services
{
    public class LocalizationService : ILocalizationService
    {
        private readonly IStructLanguageRepository _languageRepository;
        public LocalizationService(
            IStructLanguageRepository languageRepository)
        {
            _languageRepository = languageRepository;
        }

        public T GetLocalizedValue<T>(Attribute attribute, dynamic value, string culture)
        {
            if (attribute.Localized && value is JArray valueArray)
            {
                var localizedValues = valueArray.ToObject<List<LocalizedData<T>>>();
                var localizedValue = localizedValues.FirstOrDefault(x => x.CultureCode.Equals(culture));
                if (localizedValue != null
                    && localizedValue.Data != null
                    && !string.IsNullOrWhiteSpace(localizedValue.Data.ToString()))
                {
                    return localizedValue.Data;
                }
                
                if (attribute.FallbackLanguage.HasValue)
                {
                    var fallbackLanguage = _languageRepository.GetLanguage(attribute.FallbackLanguage.Value);
                    if (fallbackLanguage != null)
                    {
                        var fallbackLocalizedValue =
                            localizedValues.FirstOrDefault(x => x.CultureCode.Equals(fallbackLanguage.CultureCode));
                        if (fallbackLocalizedValue != null)
                        {
                            return fallbackLocalizedValue.Data;
                        }
                    }
                }

                return default(T);
            }
            else if (value is T parsedValue)
            {
                return parsedValue;
            }
            else if (value is JObject parsedObj)
            {
                return parsedObj.ToObject<T>();
            }
            else if (value is JToken parsedToken)
            {
                return parsedToken.ToObject<T>();
            }

            return default(T);
        }
    }
}
