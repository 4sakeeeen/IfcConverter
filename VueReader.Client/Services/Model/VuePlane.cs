using IfcConverter.Client.Services.Model.Base;
using IfcConverter.Client.Services.Model.Common;
using IngrDataReadLib;
using System;
using System.Collections.Generic;
using Xbim.Common;
using Xbim.Ifc2x3.FacilitiesMgmtDomain;
using Xbim.Ifc4.GeometricModelResource;
using Xbim.Ifc4.GeometryResource;
using Xbim.Ifc4.TopologyResource;

namespace IfcConverter.Client.Services.Model
{
    public sealed class VuePlane : VueGraphicElement
    {
        public VuePosition Normal { get; }

        public VuePosition UDirection { get; }

        public List<VueBoundary> Bounds { get; }

        public VuePlane(int aspectNo, int sequenceInGroup, IRdPlane plane)
            : base(aspectNo, sequenceInGroup)
        {
            RdBoundary? boundary = null;
            plane.GetNormal(out Position normal);
            plane.GetuDirection(out Position dir);
            plane.ReadBoundary(ref boundary);

            try
            {
                this.Normal = new VuePosition(normal);
                this.UDirection = new VuePosition(dir);
                this.Bounds = VueService.ConvertAllBoundaries(boundary);
            }
            catch (Exception ex)
            {
                throw new Exception("Convert plane: exception occurred", ex);
            }
        }

        public override IfcRepresentationItem Convert(IModel model)
        {
            return model.Instances.New<IfcFacetedBrep>(
                brep => brep.Outer = model.Instances.New<IfcClosedShell>(
                    shell => shell.CfsFaces.Add(model.Instances.New<IfcFace>(
                        face => face.Bounds.Add(model.Instances.New<IfcFaceOuterBound>(
                            bound => bound.Bound = model.Instances.New<IfcPolyLoop>(poly =>
                            {

                            })))))));
        }
    }
}
