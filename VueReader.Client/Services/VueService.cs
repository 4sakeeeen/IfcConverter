using IfcConverter.Client.Services.Model;
using IfcConverter.Client.Services.Model.Base;
using IfcConverter.Client.Services.Model.Common;
using IfcConverter.Client.Services.Model.Curves.BoundaryElements;
using IngrDataReadLib;
using System;
using System.Collections.Generic;
using System.Linq;

namespace IfcConverter.Client.Services
{
    public static class VueService
    {
        public static VueGroup ConvertGroupTypeGeometry(IRdGroup group)
        {
            var vueGroup = new VueGroup();
            group.GetElementCountFromGroup(out int count);

            IngrGeom? ingrGeom = null;
            RdMaterialProperties? materialProperties = null;

            for (int i = 0; i < count; i++)
            {
                group.GetAspectFromGroup(i, out int aspectNo);
                group.GetElementFromGroup(i, out eGraphicType type, ref ingrGeom, ref materialProperties);

                switch (type)
                {
                    case eGraphicType.PLANE_TYPE:
                        vueGroup.GraphicElements.Add(ConvertPlaneTypeGeometry(aspectNo, i, ingrGeom as IRdPlane ?? throw new Exception()));
                        break;

                    default:
                        break;
                }
            }

            return vueGroup;
        }

        public static VuePlane ConvertPlaneTypeGeometry(int aspectNo, int sequenceInGroup, IRdPlane plane)
        {
            RdBoundary? boundary = null;
            plane.GetNormal(out Position normal);
            plane.GetuDirection(out Position dir);
            plane.ReadBoundary(ref boundary);

            return new VuePlane(
                aspectNo, sequenceInGroup,
                normal: ConvertPosition(normal),
                uDirection: ConvertPosition(dir),
                vueBoundaries: ConvertBoundaries(boundary));
        }

        public static List<VueBoundary> ConvertBoundaries(IRdBoundary boundary)
        {
            var vueBoundaries = new List<VueBoundary>();
            boundary.GetBoundaryCount(out int boundaryCount);

            for (int i = 0; i < boundaryCount; i++)
            {
                var vueBoundary = new VueBoundary();
                vueBoundaries.Add(vueBoundary);
                boundary.GetBoundaryCurveCount(i, out int curveCount);
                
                for (int k = 0; k < curveCount; k++)
                {
                    IngrGeom? geom = null;
                    boundary.GetBoundaryCurve(boundaryCount, curveCount, geom, out eGraphicType type);

                    switch (type)
                    {
                        case eGraphicType.LINE_TYPE:
                            vueBoundary.Curves.Add(ConvertBoundaryLine(k, geom as IRdLine ?? throw new Exception()));
                            break;

                        case eGraphicType.LINESTRING_TYPE:
                            vueBoundary.Curves.Add(ConvertBoundaryLineString(k, geom as IRdLineString ?? throw new Exception()));
                            break;

                        default:
                            break;
                    }
                }
            }

            return vueBoundaries;
        }


        public static VueBoundaryLineString ConvertBoundaryLineString(int sequenceInBoundary, IRdLineString lineString)
        {
            lineString.GetVertices(out Array vertices, out int vertixCount);
            return new VueBoundaryLineString(sequenceInBoundary, vertices.Cast<Position>().Select(x => new VuePosition(x.m_xPosition, x.m_yPosition, x.m_xPosition)));
        }

        public static VueBoundaryLine ConvertBoundaryLine(int sequenceInBoundary, IRdLine line)
        {
            line.GetLineEndPoints(out Position startPoint, out Position endPoint);
            return new VueBoundaryLine(sequenceInBoundary, startPoint: ConvertPosition(startPoint), endPoint: ConvertPosition(endPoint));
        }

        public static VuePosition ConvertPosition(Position position)
        {
            return new VuePosition(position.m_xPosition, position.m_yPosition, position.m_zPosition);
        }
    }
}
