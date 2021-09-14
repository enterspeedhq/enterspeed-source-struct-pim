using Enterspeed.Integration.Struct.Models.Configuration;

namespace Enterspeed.Integration.Struct.Services
{
    public interface IEnterspeedConfigurationService
    {
        EnterspeedStructConfiguration GetConfiguration();
    }
}
