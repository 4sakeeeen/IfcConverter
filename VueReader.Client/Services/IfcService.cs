using System;
using System.Linq;
using VueReader.Domain.Models.Vue.Common.Collections;
using Xbim.Common;
using Xbim.Ifc;
using Xbim.Ifc4.GeometricModelResource;
using Xbim.Ifc4.GeometryResource;
using Xbim.Ifc4.RepresentationResource;
using Xbim.Ifc4.SharedBldgElements;
using Xbim.Ifc4.TopologyResource;

namespace VueReader.Client.Services
{
    public class IfcService
    {
        private readonly IfcStore _Model;

        private IfcRepresentationContext? RepresentationContext => _Model.Instances.OfType<IfcGeometricRepresentationContext>().FirstOrDefault();

        public IfcService(IfcStore model) { _Model = model; }

        public IfcShapeRepresentation CreateShapeRepresentation(Elements geometryElements)
        {
            return _Model.Instances.New<IfcShapeRepresentation>(x =>
            {
                x.ContextOfItems = RepresentationContext;
                geometryElements.Planes.ForEach(plane =>
                {
                    x.Items.Add(_Model.Instances.New<IfcFacetedBrep>(brep =>
                    {
                        brep.Outer = _Model.Instances.New<IfcClosedShell>(shell =>
                        {
                            shell.CfsFaces.Add(_Model.Instances.New<IfcFace>(face =>
                            {
                                face.Bounds.Add(_Model.Instances.New<IfcFaceOuterBound>(bound =>
                                {
                                    bound.Bound = _Model.Instances.New<IfcPolyLoop>(loop =>
                                    {
                                        if (plane.Boundaries == null) { throw new Exception("Boundary of plane is null"); }
                                        plane.GetAllPositions().ToList().ForEach(p => loop.Polygon.Add(_Model.Instances.New<IfcCartesianPoint>(point => point.SetXYZ(p.X * 1000, p.Y * 1000, p.Z * 1000))));
                                    });
                                }));
                            }));
                        });
                    }));
                });
            });
        }
    }
}
