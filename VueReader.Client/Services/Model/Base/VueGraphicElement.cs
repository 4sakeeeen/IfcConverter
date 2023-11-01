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
    }
}
