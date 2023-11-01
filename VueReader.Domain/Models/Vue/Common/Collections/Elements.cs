using System.Text.Json.Serialization;
using IfcConverter.Domain.Models.Vue.Geometries;

namespace IfcConverter.Domain.Models.Vue.Common.Collections
{
    public class Elements
    {
        [JsonPropertyName("PLANE_TYPE")]
        public List<Plane> Planes { get; set; } = new();

        [JsonPropertyName("LINE_TYPE")]
        public List<Line> Lines { get; set; } = new();

        [JsonPropertyName("PROJECTION_TYPE")]
        public List<Projection> Projections { get; set; } = new();
    }
}
