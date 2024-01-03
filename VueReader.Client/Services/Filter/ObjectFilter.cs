using System.Text.Json.Serialization;

namespace IfcConverter.Client.Services.Filter
{
    public sealed class ObjectFilter
    {
        [JsonPropertyName("name")]
        public string? Name { get; set; }

        [JsonPropertyName("criteria")]
        public string? Criteria { get; set; }
    }
}
