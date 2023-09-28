namespace VueReader.Domain.Models.BuiltIn
{
    /// <summary>
    /// The Position structure contains three double values. This structure represents the vertex data position and other 3D data.
    /// </summary>
    public struct Position
    {
        public double X;
        public double Y;
        public double Z;

        public Position(double x, double y, double z)
        {
            X = x;
            Y = y;
            Z = z;
        }
    }
}
