using System.Text.Json.Serialization;
using IfcConverter.Domain.Models.Vue.Common;
using IfcConverter.Domain.Models.Vue.Common.Collections;

namespace IfcConverter.Domain.Models.Vue.Geometries
{
    public class Plane
    {
        [JsonPropertyName("Element No.")]
        public int ElementNo { get; set; }

        [JsonPropertyName("Aspect No.")]
        public int AspectNo { get; set; }

        [JsonPropertyName("Normal")]
        public Vector3D? Normal { get; set; }

        [JsonPropertyName("uDirection")]
        public Vector3D? UDirection { get; set; }

        [JsonPropertyName("boundaries")]
        public Boundaries? Boundaries { get; set; }

        public IEnumerable<Position3D> GetAllPositions()
        {
            if (Boundaries == null) { throw new Exception("No boundaries found"); }

            List<Position3D> positions = new();

            Boundaries.Lines.ForEach(x =>
            {
                if (x.StartPoint != null && !positions.Contains(x.StartPoint)) { positions.Add(x.StartPoint); }
                if (x.EndPoint != null && !positions.Contains(x.EndPoint)) { positions.Add(x.EndPoint); }
            });

            return positions;
        }
    }
}
