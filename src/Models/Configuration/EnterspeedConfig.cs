using System;
using System.Collections.Generic;
using Enterspeed.Integration.Struct.Services.StructAttributes;

namespace Enterspeed.Integration.Struct.Models.Configuration
{
    public class EnterspeedConfig
    {
        public List<Type> StructAttributeValueConverters { get; } = new List<Type>();

        public EnterspeedConfig AppendValueConverter<T>()
        where T : IStructAttributeValueConverter
        {
            StructAttributeValueConverters.Add(typeof(T));
            return this;
        }
    }
}
