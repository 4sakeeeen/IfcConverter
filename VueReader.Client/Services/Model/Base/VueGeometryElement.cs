using System.Drawing;

namespace IfcConverter.Client.Services.Model.Base
{
    public abstract class VueGeometryElement
    {
        public int AspectNo { get; }

        public int SequenceInGroup { get; }

        public Color Color { get; set; }

        public double Transparency { get; set; }

        public VueGeometryElement(int aspectNo, int sequenceInGroup)
        {
            AspectNo = aspectNo;
            SequenceInGroup = sequenceInGroup;
        }
    }
}
