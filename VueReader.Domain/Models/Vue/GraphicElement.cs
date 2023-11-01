using IfcConverter.Domain.Models.Vue.Common;
using System.Text.Json.Serialization;

namespace IfcConverter.Domain.Models.Vue
{
    public class GraphicElement
    {
        [JsonPropertyName("smart3dClass")]
        [JsonConverter(typeof(JsonStringEnumConverter))]
        public SmartClassID Smart3DClass { get; set; }

        [JsonPropertyName("labelProperties")]
        public Dictionary<string, string?> LabelProperties { get; set; } = new();

        [JsonPropertyName("GROUP_TYPE")]
        public GeometryGroup Geometry { get; set; } = new();
    }
}
