using IfcConverter.Client.Services.Model.Base;
using IngrDataReadLib;
using System;
using System.Collections.Generic;
using System.Linq;
using Xbim.Common;
using Xbim.Ifc4.GeometryResource;
using Xbim.Ifc4.RepresentationResource;

namespace IfcConverter.Client.Services.Model
{
    public sealed class VueGroup : IConvertable<IfcShapeRepresentation>
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
                        this._GeometryElements.Add(new VueTessellatedElement(aspectNo, i, tessData));
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
                    throw new Exception($"Convert geometry group: one of graphic element can not be converted", ex);
                }
            }
        }


        public IfcShapeRepresentation Convert(IModel model)
        {
            return model.Instances.New(delegate (IfcShapeRepresentation representation)
            {
                representation.ContextOfItems = model.Instances.OfType<IfcGeometricRepresentationContext>().FirstOrDefault();
                representation.RepresentationType = "SolidModel";
                representation.RepresentationIdentifier = "Body";
                this._GeometryElements.ForEach(delegate (VueGeometryElement convertable)
                {
                    representation.Items.Add(((IConvertable<IfcGeometricRepresentationItem>)convertable).Convert(model));
                });
            });
        }
    }
}
