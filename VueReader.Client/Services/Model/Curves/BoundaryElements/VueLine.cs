using IfcConverter.Client.Services.Model.Base;
using IngrDataReadLib;

namespace IfcConverter.Client.Services.Model.Curves.BoundaryElements
{
    public sealed class VueLine : IBoundaryElement
    {
        public VuePosition StartPoint { get; }

        public VuePosition EndPoint { get; }

        public VueLine(IRdLine line)
        {
            line.GetLineEndPoints(out Position startPoint, out Position endPoint);
            this.StartPoint = new VuePosition(startPoint);
            this.EndPoint = new VuePosition(endPoint);
        }
    }
}
