using IfcConverter.Domain.Models.Vue.Common;
using System.Text.Json.Serialization;

namespace IfcConverter.Domain.Models.Vue.Geometries
{
    public class Projection
    {
        [JsonPropertyName("vector")]
        public Vector3D? Vector { get; set; }

        [JsonPropertyName("positions")]
        public List<Position3D> Positions { get; set; } = new();
    }
}
