using IfcConverter.Client.Services.Model.Base;
using IfcConverter.Client.Services.Model.Curves.BoundaryElements;
using IngrDataReadLib;
using System;
using Xbim.Common;
using Xbim.Ifc4.GeometricModelResource;
using Xbim.Ifc4.GeometryResource;

namespace IfcConverter.Client.Services.Model
{
    public sealed class VueProjection : VueGeometryElement, IConvertable<IfcSolidModel>
    {
        public VuePosition SweepVector { get; }

        public bool IsCapped { get; }

        public bool IsOrthogonal { get; }

        public VueProjection(int aspectNo, int sequenceInGroup, IRdProjection projection)
            : base(aspectNo, sequenceInGroup)
        {
            IngrGeom? geometry = null;
            projection.GetProjectionCurve(ref geometry, out eGraphicType type);
            projection.GetSweepVector(out Position vector);
            projection.IsCapped(out byte isCapped);
            projection.IsOrthogonal(out byte isOrthogonal);

            switch (type)
            {
                case eGraphicType.LINESTRING_TYPE:
                    new VueLineString((RdLineString)geometry);
                    break;

                case eGraphicType.ELLIPSE_TYPE:
                    break;

                case eGraphicType.COMPLEXSTRING_TYPE:
                    break;

                default:
                    throw new Exception("");
            }
            
            this.SweepVector = new VuePosition(vector);
            this.IsCapped = isCapped == 1;
            this.IsOrthogonal = isOrthogonal == 1;
        }

        public IfcSolidModel Convert(IModel model)
        {
            throw new NotImplementedException();
        }
    }
}
