using System.Text.Json.Serialization;

namespace IfcConverter.Domain.Models.Vue.Common
{
    public class Location
    {
        [JsonPropertyName("E")]
        public string? E { get; set; }

        [JsonPropertyName("N")]
        public string? N { get; set; }

        [JsonPropertyName("EL+")]
        public string? EL { get; set; }
    }
}
