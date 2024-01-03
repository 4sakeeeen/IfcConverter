using IfcConverter.Client.Services.Model.Base;
using IngrDataReadLib;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using Xbim.Common;
using Xbim.Ifc4.GeometricModelResource;
using Xbim.Ifc4.GeometryResource;
using Xbim.Ifc4.Interfaces;
using Xbim.Ifc4.PresentationAppearanceResource;
using Xbim.Ifc4.TopologyResource;

namespace IfcConverter.Client.Services.Model
{
    public sealed class VueTessellatedElement : VueGeometryElement, IConvertable<IfcGeometricRepresentationItem>
    {
        private readonly StripData _StripData;


        public VueTessellatedElement(int aspectNo, int sequenceInGroup, IRdTessData tessData, IRdMaterialProperties materialProperties)
            : base(aspectNo, sequenceInGroup)
        {
            var postemp = new Position2d();
            tessData.GetStripData(out StripData stripData);
            _StripData = stripData;

            materialProperties.GetColor(out byte red, out byte green, out byte blue);
            Color = Color.FromArgb(red, green, blue);
            materialProperties.GetTransparency(out double transparency);
            Transparency = transparency;
            // materialProperties.GetMaterialData(out Array meterial);
        }

        /// <exception cref="Exception"></exception>
        public IfcGeometricRepresentationItem Convert(IModel model)
        {
            IfcGeometricRepresentationItem geometricRepresentation = (eGraphicType)_StripData.mType switch
            {
                eGraphicType.LINE_STRIP => model.Instances.New<IfcPolyline>(polyline =>
                {
                    polyline.Points.AddRange(
                        _StripData.mLineStrip.m_pVertex
                        .Cast<Position>()
                        .Select(position =>
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
                        IEnumerable<Position> vertices = _StripData.mTriStrip.m_pVertex.Cast<Position>();

                        foreach (IEnumerable<int> triangleByIndices in Chunk(_StripData.mTriStrip.m_pVertexIndex.Cast<int>(), 3))
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
                _ => throw new Exception($"Invalid strip data: {(eGraphicType)_StripData.mType}")
            };

            IfcPresentationStyle style = (eGraphicType)_StripData.mType switch
            {
                eGraphicType.LINE_STRIP => model.Instances.New<IfcCurveStyle>(curveStyle =>
                {
                    curveStyle.CurveColour = model.Instances.New<IfcColourRgb>(color =>
                    {
                        color.Red = 1;
                        color.Green = 1;
                        color.Blue = 1;
                    });
                }),
                eGraphicType.TRIANGLE_STRIP => model.Instances.New<IfcSurfaceStyle>(surfaceStyle =>
                {
                    surfaceStyle.Side = IfcSurfaceSide.BOTH;
                    surfaceStyle.Styles.Add(model.Instances.New<IfcSurfaceStyleRendering>(rendering =>
                    {
                        rendering.Transparency = Transparency;
                        rendering.SurfaceColour = model.Instances.New<IfcColourRgb>(color =>
                        {
                            color.Red = Color.R / 255.0;
                            color.Green = Color.G / 255.0;
                            color.Blue = Color.B / 255.0;
                        });
                    }));
                }),
                _ => throw new Exception("Style error")
            };

            model.Instances.New<IfcStyledItem>(styled =>
            {
                styled.Item = geometricRepresentation;
                styled.Styles.Add(style);
            });

            return geometricRepresentation;
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
