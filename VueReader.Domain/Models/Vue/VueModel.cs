using System.Text.Json.Serialization;

namespace VueReader.Domain.Models.Vue
{
    internal class VueModel
    {
        [JsonPropertyName("graphicElements")]
        public List<GraphicElement> GraphicElements { get; set; } = new();
    }
}
