using System;
using Xbim.Common;
using Xbim.Ifc4.GeometryResource;

namespace IfcConverter.Client.Services.Model.Base
{
    public abstract class VueGraphicElement
    {
        public int AspectNo { get; }

        public int SequenceInGroup { get; }

        public VueGraphicElement(int aspectNo, int sequenceInGroup)
        {
            AspectNo = aspectNo;
            SequenceInGroup = sequenceInGroup;
        }

        public virtual IfcRepresentationItem Convert(IModel model)
        {
            throw new NotImplementedException();
        }
    }
}
