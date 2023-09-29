using System.Text.Json.Serialization;

namespace IfcConverter.Domain.Models.Vue.Common
{
    public class Vector3D
    {
        [JsonPropertyName("X")]
        public int X { get; set; }

        [JsonPropertyName("Y")]
        public int Y { get; set; }

        [JsonPropertyName("Z")]
        public int Z { get; set; }
    }
}
