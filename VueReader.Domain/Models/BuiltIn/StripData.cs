namespace VueReader.Domain.Models.BuiltIn
{
    /// <summary>
    /// The StripData structure represents the format of the tessellated data that can be linear or surface data.The mType parameter represents the type of data this structure contains. For
    /// example: if mType is LINE_STRIP, then it is LineStrip data. Similarly, TRIANGLE_STRIP represents the TriStrip data.
    /// </summary>
    public struct StripData
    {
        public TriStrip TriangleStrip;
        public LineStrip LineStrip;
        public long Type;

        public StripData(TriStrip triangleStrip, LineStrip lineStrip, long type)
        {
            TriangleStrip = triangleStrip;
            LineStrip = lineStrip;
            Type = type;
        }
    }
}
