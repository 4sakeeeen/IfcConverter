using IfcConverter.Client.Services.Model.Base;
using IngrDataReadLib;
using Xbim.Common;
using Xbim.Ifc4.GeometryResource;

namespace IfcConverter.Client.Services.Model.Curves.BoundaryElements
{
    public sealed class VueLine : IBoundaryElement, IIfcConvertable<IfcLine>
    {
        public VuePosition StartPoint { get; }

        public VuePosition EndPoint { get; }

        public VueLine(IRdLine line)
        {
            line.GetLineEndPoints(out Position startPoint, out Position endPoint);
            this.StartPoint = new VuePosition(startPoint);
            this.EndPoint = new VuePosition(endPoint);
        }

        public IfcLine IfcConvert(IModel model)
        {
            throw new System.NotImplementedException();
        }
    }
}
