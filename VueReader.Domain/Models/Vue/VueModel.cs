using System.Text.Json.Serialization;

namespace VueReader.Domain.Models.Vue
{
    public class VueModel
    {
        [JsonPropertyName("graphicElements")]
        public List<GraphicElement> GraphicElements { get; set; } = new();
    }
}
