using System.Text.Json.Serialization;
using VueReader.Domain.Models.Vue.Geometries;

namespace VueReader.Domain.Models.Vue.Common.Collections
{
    public class Elements
    {
        [JsonPropertyName("PLANE_TYPE")]
        public List<Plane> Planes { get; set; } = new();
    }
}
