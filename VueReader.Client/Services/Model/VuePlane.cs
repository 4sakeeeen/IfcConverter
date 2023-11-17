using IfcConverter.Client.Services.Model.Base;
using IfcConverter.Client.Services.Model.Common;
using IngrDataReadLib;
using System;
using System.Collections.Generic;
using Xbim.Common;
using Xbim.Ifc4.GeometricModelResource;
using Xbim.Ifc4.GeometryResource;

namespace IfcConverter.Client.Services.Model
{
    public sealed class VuePlane : VueGeometryElement, IConvertable<IfcFacetedBrep>
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

        public IfcFacetedBrep Convert(IModel model)
        {
            throw new NotImplementedException();
        }
    }
}
