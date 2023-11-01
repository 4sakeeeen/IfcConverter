using System.Text.Json.Serialization;
using IfcConverter.Domain.Models.Vue.Common.Collections;

namespace IfcConverter.Domain.Models.Vue
{
    public class GeometryGroup
    {
        [JsonPropertyName("elements")]
        public Elements Elements { get; set; } = new();
    }
}
