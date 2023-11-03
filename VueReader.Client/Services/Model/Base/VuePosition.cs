using IngrDataReadLib;
using Xbim.Common;
using Xbim.Ifc4.GeometryResource;

namespace IfcConverter.Client.Services.Model.Base
{
    public sealed class VuePosition
    {
        public double X { get; }

        public double Y { get; }

        public double Z { get; }


        public VuePosition(Position position)
        {
            this.X = position.m_xPosition;
            this.Y = position.m_yPosition;
            this.Z = position.m_zPosition;
        }


        public IfcCartesianPoint ToIFC(IModel model)
        {
            return model.Instances.New<IfcCartesianPoint>(point => point.SetXYZ(this.X, this.Y, this.Z));
        }
    }
}
