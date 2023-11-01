using IfcConverter.Client.Services.Model.Base;
using System.Collections.Generic;
using System.Linq;

namespace IfcConverter.Client.Services.Model.Curves.BoundaryElements
{
    public sealed class VueBoundaryLineString : VueBoundaryElement
    {
        public List<VuePosition> Vertices { get; } = new();

        public VueBoundaryLineString(int sequenceInBoundary, IEnumerable<VuePosition> vertices) : base(sequenceInBoundary)
        {
            this.Vertices = vertices.ToList();
        }
    }
}
