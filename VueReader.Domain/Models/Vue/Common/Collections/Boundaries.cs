using System.Text.Json.Serialization;
using VueReader.Domain.Models.Vue.Geometries;

namespace VueReader.Domain.Models.Vue.Common.Collections
{
    public class Boundaries
    {
        [JsonPropertyName("LINE_TYPE")]
        public List<Line> Lines { get; set; } = new();
    }
}
