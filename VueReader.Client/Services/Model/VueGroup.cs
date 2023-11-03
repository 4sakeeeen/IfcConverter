using IfcConverter.Client.Services.Model.Base;
using IngrDataReadLib;
using System;
using System.Collections.Generic;
using System.Linq;
using Xbim.Common;
using Xbim.Ifc4.RepresentationResource;

namespace IfcConverter.Client.Services.Model
{
    public sealed class VueGroup : IIfcConvertable<IfcShapeRepresentation>
    {
        public List<VueGraphicElement> GraphicElements { get; } = new();

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
                    switch (type)
                    {
                        case eGraphicType.PLANE_TYPE:
                            GraphicElements.Add(new VuePlane(aspectNo, i, (IRdPlane)geometry));
                            break;

                        case eGraphicType.PROJECTION_TYPE:
                            GraphicElements.Add(new VueProjection(aspectNo, i, (IRdProjection)geometry));
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
                catch (Exception ex)
                {
                    throw new Exception($"Convert geometry group: one of graphic element can not be converted", ex);
                }
            }

        }

        public IfcShapeRepresentation IfcConvert(IModel model)
        {
            return model.Instances.New<IfcShapeRepresentation>(shrep =>
            {
                shrep.ContextOfItems = model.Instances.OfType<IfcGeometricRepresentationContext>().FirstOrDefault();
                GraphicElements.ForEach(x => shrep.Items.Add(x.Convert(model)));
            });
        }
    }
}
