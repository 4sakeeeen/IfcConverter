namespace IfcConverter.Client.Services.Model.Base
{
    public sealed class VuePosition
    {
        public double X { get; }

        public double Y { get; }

        public double Z { get; }

        public VuePosition(double x, double y, double z)
        {
            this.X = x;
            this.Y = y;
            this.Z = z;
        }
    }
}
