using IfcConverter.Client.Services.Model.Base;

namespace IfcConverter.Client.Services.Model.Curves.BoundaryElements
{
    public sealed class VueBoundaryLine : VueBoundaryElement
    {
        public VuePosition StartPoint { get; }

        public VuePosition EndPoint { get; }

        public VueBoundaryLine(int sequenceInBoundary, VuePosition startPoint, VuePosition endPoint)
            : base(sequenceInBoundary)
        {
            StartPoint = startPoint;
            EndPoint = endPoint;
        }
    }
}
