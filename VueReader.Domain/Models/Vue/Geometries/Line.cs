using IfcConverter.Domain.Models.Vue.Common;
using System.Text.Json.Serialization;

namespace IfcConverter.Domain.Models.Vue.Geometries
{
    public class Line
    {
        [JsonPropertyName("Element No.")]
        public int ElementNo { get; set; }

        [JsonPropertyName("Aspect No.")]
        public int AspectNo { get; set; }

        [JsonPropertyName("Start Point")]
        public Position3D? StartPoint { get; set; }

        [JsonPropertyName("End Point")]
        public Position3D? EndPoint { get; set; }
    }
}
