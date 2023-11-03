using IfcConverter.Client.Services.Model.Base;
using IngrDataReadLib;

namespace IfcConverter.Client.Services.Model.Curves
{
    public sealed class VueArcEllipse : IBoundaryElement
    {
        public double StartAngle { get; }

        public double EndAngle { get; }

        public VuePosition MajorAxis { get; }

        public double Ratio { get; }

        public VuePosition Centre { get; }

        public VuePosition Normal { get; }

        public VueArcEllipse(IRdArcEllipse arcEllipse)
        {
            arcEllipse.GetAngles(out double startAngle, out double endAngle);
            arcEllipse.GetMajorAxis(out Position majorAxis, out double ratio);
            arcEllipse.GetCentreAndNormal(out Position centre, out Position normal);
            this.StartAngle = startAngle;
            this.EndAngle = endAngle;
            this.MajorAxis = new VuePosition(majorAxis);
            this.Ratio = ratio;
            this.Centre = new VuePosition(centre);
            this.Normal = new VuePosition(normal);
        }
    }
}
