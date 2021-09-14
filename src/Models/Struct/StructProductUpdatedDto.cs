using System.Collections.Generic;

namespace Enterspeed.Integration.Struct.Models.Struct
{
    public class StructProductUpdatedDto
    {
        public int Id { get; set; }
        public List<string> UpdatedAttributes { get; set; }
    }
}
