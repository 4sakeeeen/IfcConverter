using IfcConverter.Client.Services.Model.Base;
using IngrDataReadLib;
using System;
using System.Collections.Generic;
using System.Linq;

namespace IfcConverter.Client.Services.Model.Curves.BoundaryElements
{
    public sealed class VueLineString : IBoundaryElement
    {
        public List<VuePosition> Vertices { get; }

        public VueLineString(IRdLineString lineString)
        {
            lineString.GetVertices(out Array vertices, out int vertixCount);
            Vertices = vertices.Cast<Position>().Select(x => new VuePosition(x)).ToList();
        }
    }
}
