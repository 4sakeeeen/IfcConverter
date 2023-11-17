namespace IfcConverter.Client.Services.Model.Base
{
    public abstract class VueGeometryElement
    {
        public int AspectNo { get; }

        public int SequenceInGroup { get; }

        public VueGeometryElement(int aspectNo, int sequenceInGroup)
        {
            this.AspectNo = aspectNo;
            this.SequenceInGroup = sequenceInGroup;
        }
    }
}
