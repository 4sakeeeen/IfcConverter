using IfcConverter.Client.Services.Model.Base;
using IngrDataReadLib;
using Serilog;
using System;
using System.CodeDom;
using System.Collections.Generic;
using System.Linq;
using Xbim.Common;
using Xbim.Ifc4.GeometricModelResource;
using Xbim.Ifc4.GeometryResource;
using Xbim.Ifc4.PresentationAppearanceResource;
using Xbim.Ifc4.RepresentationResource;

namespace IfcConverter.Client.Services.Model
{
    public sealed class VueGroup : IConvertable<IEnumerable<IfcShapeRepresentation>>
    {
        private readonly List<VueGeometryElement> _GeometryElements = new();


        public VueGroup(IRdGroup group)
        {
            group.GetElementCountFromGroup(out int count);
            IngrGeom? geometry = null;
            RdMaterialProperties? materialProperties = null;

            for (int i = 0; i < count; i++)
            {
                group.GetAspectFromGroup(i, out int aspectNo);
                group.GetElementFromGroup(i, out eGraphicType type, ref geometry, ref materialProperties);

                try
                {
                    if (geometry is IRdTessData tessData)
                    {
                        _GeometryElements.Add(new VueTessellatedElement(aspectNo, i, tessData, materialProperties));
                    }
                    else
                    {
                        switch (type)
                        {
                            case eGraphicType.PLANE_TYPE:
                                // new VuePlane(aspectNo, i, (IRdPlane)geometry);
                                break;

                            case eGraphicType.PROJECTION_TYPE:
                                // new VueProjection(aspectNo, i, (IRdProjection)geometry);
                                break;

                            case eGraphicType.LINESTRING_TYPE:
                                break;

                            case eGraphicType.CONE_TYPE:
                                break;

                            case eGraphicType.REVOLUTION_TYPE:
                                break;

                            case eGraphicType.LINE_TYPE:
                                break;

                            case eGraphicType.RULED_TYPE:
                                break;

                            default:
                                break;
                        }
                    }
                }
                catch (Exception ex)
                {
                    throw new Exception($"Convert geometry group: one of graphic element can not be converted.", ex);
                }
            }
        }

        /// <exception cref="Exception"></exception>
        public IEnumerable<IfcShapeRepresentation> Convert(IModel model)
        {
            var geometricRepItems = new List<IfcGeometricRepresentationItem>();

            foreach (VueGeometryElement geometryElement in _GeometryElements)
            {
                try
                {
                    geometricRepItems.Add(((IConvertable<IfcGeometricRepresentationItem>)geometryElement).Convert(model));
                }
                catch (Exception ex)
                {
                    Log.Logger.Error(ex, "Convert geometry group failed. One of contains elements can not be converted.");
                }
            }

            foreach (IGrouping<Type, IfcGeometricRepresentationItem> geometryGroupByType in geometricRepItems.GroupBy(e => e.GetType()))
            {
                var representation = model.Instances.New<IfcShapeRepresentation>();
                representation.Items.AddRange(geometryGroupByType);
                representation.ContextOfItems = model.Instances.OfType<IfcGeometricRepresentationContext>().FirstOrDefault();
                representation.RepresentationIdentifier = "Body";
                representation.RepresentationType = geometryGroupByType.Key.Name switch
                {
                    "IfcPolyline" => "Curve3D",
                    "IfcFacetedBrep" => "Brep",
                    _ => throw new Exception($"Type {geometryGroupByType.Key.Name} invalid for converting to shape representation.")
                };

                yield return representation;
            }
        }
    }
}
