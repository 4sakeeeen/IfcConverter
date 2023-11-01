using System.Text.Json.Serialization;

namespace IfcConverter.Domain.Models.Vue
{
    public class VueModel
    {
        [JsonPropertyName("graphicElements")]
        public Dictionary<string, GraphicElement> GraphicElements { get; set; } = new();
    }
}
