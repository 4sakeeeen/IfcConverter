using System.Text.Json.Serialization;
using IfcConverter.Domain.Models.Vue.Common.Collections;

namespace IfcConverter.Domain.Models.Vue
{
    public class GeometryGroup
    {
        [JsonPropertyName("Color")]
        public string? Color { get; set; }

        [JsonPropertyName("Transparency")]
        public int Transparency { get; set; }

        [JsonPropertyName("Base Specular Color")]
        public string? BaseSpecularColor { get; set; }

        [JsonPropertyName("Ambient Color")]
        public string? AmbientColor { get; set; }

        [JsonPropertyName("Diffuse Color")]
        public string? DiffuseColor { get; set; }

        [JsonPropertyName("Specular Color")]
        public string? SpecularColor { get; set; }

        [JsonPropertyName("Wireframe Color")]
        public string? WireframeColor { get; set; }

        [JsonPropertyName("Reflection")]
        public double Reflection { get; set; }

        [JsonPropertyName("Finish")]
        public double Finish { get; set; }

        [JsonPropertyName("Transmit")]
        public double Transmit { get; set; }

        [JsonPropertyName("Line Width")]
        public double LineWidth { get; set; }

        [JsonPropertyName("Stipple Scale")]
        public int StippleScale { get; set; }

        [JsonPropertyName("Stipple Pattern")]
        public int StipplePattern { get; set; }

        [JsonPropertyName("Pattern Filename")]
        public object? PatternFilename { get; set; }

        [JsonPropertyName("No. Of Elements")]
        public int NoOfElements { get; set; }

        [JsonPropertyName("elements")]
        public Elements? Elements { get; set; }
    }

}
