namespace IfcConverter.Client.Services.Model.Base
{
    public abstract class VueBoundaryElement
    {
        public int SequenceInBoundary { get; }

        public VueBoundaryElement(int sequenceInBoundary)
        {
            this.SequenceInBoundary = sequenceInBoundary;
        }
    }
}
