namespace IfcConverter.Domain.Models.BuiltIn
{
    /// <summary>
    /// The Facet structure contains Face index for a triangle face (m_i4 = 0) or a quad. This structure contains information about the POLYMESH_TYPE faces.
    /// </summary>
    public struct Facet
    {
        public long I1;
        public long I2;
        public long I3;
        public long I4;

        public Facet(long i1, long i2, long i3, long i4)
        {
            I1 = i1;
            I2 = i2;
            I3 = i3;
            I4 = i4;
        }
    }
}
