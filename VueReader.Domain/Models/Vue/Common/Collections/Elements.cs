using System.Text.Json.Serialization;
using IfcConverter.Domain.Models.Vue.Geometries;

namespace IfcConverter.Domain.Models.Vue.Common.Collections
{
    public class Elements
    {
        [JsonPropertyName("PLANE_TYPE")]
        public List<Plane> Planes { get; set; } = new();
    }
}
