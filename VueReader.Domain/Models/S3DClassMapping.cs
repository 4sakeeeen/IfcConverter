using System.Text.Json.Serialization;

namespace IfcConverter.Domain.Models
{
    public class S3DClassMapping
    {
        [JsonPropertyName("category")]
        public string Caterory { get; set; }

        [JsonPropertyName("className")]
        public string ClassName { get; set; }

        [JsonPropertyName("mappedToClassIFC")]
        public string MappedClassIFC { get; set; }

        public S3DClassMapping(string caterory, string className, string mappedClassIFC)
        {
            Caterory = caterory;
            ClassName = className;
            MappedClassIFC = mappedClassIFC;
        }
    }
}
