using System.Text.Json.Serialization;

namespace IfcConverter.Domain.Models.Vue
{
    public class GraphicElement
    {
        [JsonPropertyName("labelProperties")]
        public Dictionary<string, object> LabelProperties { get; set; } = new();

        [JsonPropertyName("GROUP_TYPE")]
        public GeometryGroup? Geometry { get; set; }
    }
}
