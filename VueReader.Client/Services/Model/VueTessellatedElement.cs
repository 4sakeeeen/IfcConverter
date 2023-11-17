using IfcConverter.Client.Services.Model.Base;
using IngrDataReadLib;
using System;
using System.Collections.Generic;
using System.Linq;
using Xbim.Common;
using Xbim.Ifc4.GeometricModelResource;
using Xbim.Ifc4.GeometryResource;
using Xbim.Ifc4.TopologyResource;

namespace IfcConverter.Client.Services.Model
{
    public class VueTessellatedElement : VueGeometryElement, IConvertable<IfcGeometricRepresentationItem>
    {
        private readonly StripData _StripData;


        public VueTessellatedElement(int aspectNo, int sequenceInGroup, IRdTessData tessData)
            : base(aspectNo, sequenceInGroup)
        {
            var postemp = new Position2d();
            tessData.GetStripData(out StripData stripData);
            this._StripData = stripData;
        }


        public IfcGeometricRepresentationItem Convert(IModel model)
        {
            return (eGraphicType)this._StripData.mType switch
            {
                eGraphicType.LINE_STRIP => model.Instances.New<IfcPolyline>(polyline =>
                {
                    polyline.Points.AddRange(
                        this._StripData.mLineStrip.m_pVertex
                        .Cast<Position>()
                        .Select(delegate (Position position)
                        {
                            return model.Instances.New<IfcCartesianPoint>(point =>
                            {
                                point.SetXYZ(position.m_xPosition * 1000, position.m_yPosition * 1000, position.m_zPosition * 1000);
                            });
                        }));
                }),
                eGraphicType.TRIANGLE_STRIP => model.Instances.New<IfcFacetedBrep>(brep =>
                {
                    brep.Outer = model.Instances.New<IfcClosedShell>(shell =>
                    {
                        IEnumerable<Position> vertices = this._StripData.mTriStrip.m_pVertex.Cast<Position>();

                        foreach (IEnumerable<int> triangleByIndices in Chunk(this._StripData.mTriStrip.m_pVertexIndex.Cast<int>(), 3))
                        {
                            if (triangleByIndices.Count() != 3) { throw new Exception("Tessellated triangle indices counts failed"); }
                            shell.CfsFaces.Add(model.Instances.New<IfcFace>(face =>
                            {
                                face.Bounds.Add(model.Instances.New<IfcFaceBound>(faceBound =>
                                {
                                    faceBound.Bound = model.Instances.New<IfcPolyLoop>(poly =>
                                    {
                                        poly.Polygon.Add(model.Instances.New<IfcCartesianPoint>(point =>
                                        {
                                            var vertex = vertices.ElementAt(triangleByIndices.ElementAt(0));
                                            point.SetXYZ(vertex.m_xPosition * 1000, vertex.m_yPosition * 1000, vertex.m_zPosition * 1000);
                                        }));
                                        poly.Polygon.Add(model.Instances.New<IfcCartesianPoint>(point =>
                                        {
                                            var vertex = vertices.ElementAt(triangleByIndices.ElementAt(1));
                                            point.SetXYZ(vertex.m_xPosition * 1000, vertex.m_yPosition * 1000, vertex.m_zPosition * 1000);
                                        }));
                                        poly.Polygon.Add(model.Instances.New<IfcCartesianPoint>(point =>
                                        {
                                            var vertex = vertices.ElementAt(triangleByIndices.ElementAt(2));
                                            point.SetXYZ(vertex.m_xPosition * 1000, vertex.m_yPosition * 1000, vertex.m_zPosition * 1000);
                                        }));
                                    });
                                }));
                            }));
                        }
                    });
                }),
                _ => throw new Exception("Invalid strip data")
            };
        }

        private static IEnumerable<IEnumerable<T>> Chunk<T>(IEnumerable<T> items, int chunkSize)
        {
            while (items.Any())
            {
                yield return items.Take(chunkSize);
                items = items.Skip(chunkSize);
            }
        }
    }
}
