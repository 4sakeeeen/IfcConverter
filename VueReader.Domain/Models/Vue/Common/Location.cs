using System.Text.Json.Serialization;

namespace IfcConverter.Domain.Models.Vue.Common
{
    public class Location
    {
        [JsonPropertyName("E")]
        public double E { get; set; }

        [JsonPropertyName("N")]
        public double N { get; set; }

        [JsonPropertyName("EL+")]
        public double EL { get; set; }
    }
}
