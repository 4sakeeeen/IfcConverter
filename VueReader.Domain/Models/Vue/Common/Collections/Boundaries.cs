using System.Text.Json.Serialization;
using IfcConverter.Domain.Models.Vue.Geometries;

namespace IfcConverter.Domain.Models.Vue.Common.Collections
{
    public class Boundaries
    {
        [JsonPropertyName("LINE_TYPE")]
        public List<Line> Lines { get; set; } = new();
    }
}
