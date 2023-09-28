namespace VueReader.Domain.Models.BuiltIn
{
    /// <summary>
    /// The Position2d structure contains two double values that can be used to represent 2D data.
    /// </summary>
    public struct Position2d
    {
        public double X;
        public double Y;

        public Position2d(double x, double y)
        {
            X = x;
            Y = y;
        }
    }
}
