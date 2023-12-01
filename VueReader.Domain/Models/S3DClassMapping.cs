using System.Text.Json.Serialization;

namespace IfcConverter.Domain.Models
{
    public class S3DClassMapping
    {
        [JsonPropertyName("category")]
        public string Caterory { get; set; }

        [JsonPropertyName("id")]
        public int ID { get; set; }

        [JsonPropertyName("className")]
        public string ClassName { get; set; }

        [JsonPropertyName("mappedToClassIFC")]
        public string MappedClassIFC { get; set; }

        public S3DClassMapping(string caterory, int id, string className, string mappedClassIFC)
        {
            Caterory = caterory;
            ID = id;
            ClassName = className;
            MappedClassIFC = mappedClassIFC;
        }
    }
}
