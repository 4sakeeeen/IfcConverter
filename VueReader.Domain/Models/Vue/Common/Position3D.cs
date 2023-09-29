using System.Text.Json.Serialization;

namespace IfcConverter.Domain.Models.Vue.Common
{
    public sealed class Position3D
    {
        [JsonPropertyName("X")]
        public double X { get; set; }

        [JsonPropertyName("Y")]
        public double Y { get; set; }

        [JsonPropertyName("Z")]
        public double Z { get; set; }

        public override bool Equals(object? obj)
        {
            if (obj == null) { return false; }
            return X == ((Position3D)obj).X && Y == ((Position3D)obj).Y && Z == ((Position3D)obj).Z;
        }

        public override int GetHashCode()
        {
            return X.GetHashCode() ^ Y.GetHashCode() ^ Z.GetHashCode();
        }
    }
}
