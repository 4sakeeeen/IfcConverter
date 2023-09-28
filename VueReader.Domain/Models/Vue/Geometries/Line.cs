using System.Text.Json.Serialization;
using VueReader.Domain.Models.Vue.Common;

namespace VueReader.Domain.Models.Vue.Geometries
{
    public class Line
    {
        [JsonPropertyName("Curve No.")]
        public int CurveNo { get; set; }

        [JsonPropertyName("Start Point")]
        public Position3D? StartPoint { get; set; }

        [JsonPropertyName("End Point")]
        public Position3D? EndPoint { get; set; }
    }
}
