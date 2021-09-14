using Enterspeed.Source.Sdk.Configuration;

namespace Enterspeed.Integration.Struct.Models.Configuration
{
    public class EnterspeedStructConfiguration : EnterspeedConfiguration
    {
        public string StructApiUrl { get; set; }
        public string StructApiKey { get; set; }
    }
}
