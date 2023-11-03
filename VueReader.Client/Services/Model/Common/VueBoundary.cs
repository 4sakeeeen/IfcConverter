using IfcConverter.Client.Services.Model.Base;
using IfcConverter.Client.Services.Model.Curves;
using IfcConverter.Client.Services.Model.Curves.BoundaryElements;
using IngrDataReadLib;
using System;
using System.Collections.Generic;

namespace IfcConverter.Client.Services.Model.Common
{
    public sealed class VueBoundary
    {
        public List<IBoundaryElement> Curves { get; } = new();

        public VueBoundary(IRdBoundary boundary, int index)
        {
            boundary.GetBoundaryCurveCount(index, out int curveCount);

            for (int k = 0; k < curveCount; k++)
            {
                IngrGeom? geometry = null;
                boundary.GetBoundaryCurve(index, k, ref geometry, out eGraphicType type);

                switch (type)
                {
                    case eGraphicType.LINE_TYPE:
                        this.Curves.Add(new VueLine((IRdLine)geometry));
                        break;

                    case eGraphicType.LINESTRING_TYPE:
                        this.Curves.Add(new VueLineString((IRdLineString)geometry));
                        break;

                    case eGraphicType.ARC_TYPE:
                    case eGraphicType.ELLIPSE_TYPE:
                        this.Curves.Add(new VueArcEllipse((IRdArcEllipse)geometry));
                        break;

                    case eGraphicType.INVALID_TYPE:
                        break;
                        throw new Exception("Convert boundary: invalid curve type excepted");

                    default:
                        throw new Exception("Convert boundary: unexepted curve type");
                }
            }
        }
    }
}
