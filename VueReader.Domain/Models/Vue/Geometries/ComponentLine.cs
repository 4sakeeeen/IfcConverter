using System.Text.Json.Serialization;
using IfcConverter.Domain.Models.Vue.Common;

namespace IfcConverter.Domain.Models.Vue.Geometries
{
    public class ComponentLine
    {
        [JsonPropertyName("sequenceInBoundary")]
        public int CurveNo { get; set; }

        [JsonPropertyName("Start Point")]
        public Position3D? StartPoint { get; set; }

        [JsonPropertyName("End Point")]
        public Position3D? EndPoint { get; set; }
    }
}
