using Struct.PIM.Api.Models.Attribute;

namespace Enterspeed.Integration.Struct.Services
{
    public interface ILocalizationService
    {
        T GetLocalizedValue<T>(Attribute attribute, dynamic value, string culture);
    }
}
