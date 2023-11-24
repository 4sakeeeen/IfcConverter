using System.Text.Json.Serialization;

namespace IfcConverter.Domain.Models
{
    public class IfcClass
    {
        [JsonPropertyName("name")]
        public string Name { get; set; }

        [JsonPropertyName("inherits")]
        public List<IfcClass> Inherits { get; set; }

        public IfcClass(string name, List<IfcClass> inherits)
        {
            Name = name;
            Inherits = inherits;
        }
    }
}
