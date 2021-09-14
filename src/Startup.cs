using Enterspeed.Integration.Struct;
using Enterspeed.Integration.Struct.Configuration;
using Enterspeed.Integration.Struct.Services.StructAttributes.StructValueConverters;
using Microsoft.Azure.Functions.Extensions.DependencyInjection;

[assembly: FunctionsStartup(typeof(Startup))]
namespace Enterspeed.Integration.Struct
{
    public class Startup : FunctionsStartup
    {
        public override void Configure(IFunctionsHostBuilder builder)
        {
            builder.Services.UseStruct(structConfig =>
            {
                structConfig
                    .AppendValueConverter<AttributeReferenceAttributeValueConverter>()
                    .AppendValueConverter<BooleanAttributeValueConverter>()
                    .AppendValueConverter<CategoryReferenceAttributeValueConverter>()
                    .AppendValueConverter<CollectionReferenceAttributeValueConverter>()
                    .AppendValueConverter<ComplexAttributeValueConverter>()
                    .AppendValueConverter<DateTimeAttributeValueConverter>()
                    .AppendValueConverter<FixedListAttributeValueConverter>()
                    .AppendValueConverter<ListAttributeValueConverter>()
                    .AppendValueConverter<MediaAttributeValueConverter>()
                    .AppendValueConverter<NumberAttributeValueConverter>()
                    .AppendValueConverter<ProductReferenceAttributeValueConverter>()
                    .AppendValueConverter<TextAttributeValueConverter>()
                    .AppendValueConverter<VariantReferenceAttributeValueConverter>();
            });
        }
    }
}
