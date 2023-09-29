namespace IfcConverter.Domain.Models.BuiltIn
{
    /// <summary>
    /// The LineStrip structure represents the linear strip data for curves. The m_iVertexCount parameter represents the count of vertices.The m_pVertexIndex parameter is the index array
    /// for the vertices and the m_pVertex is the vertex array.
    /// </summary>
    public struct LineStrip
    {
        public long VertexCount;
        public long[] VertexIndexes;
        public Position[] Vertexes;

        public LineStrip(long vertexCount, long[] vertexIndexes, Position[] vertexes)
        {
            VertexCount = vertexCount;
            VertexIndexes = vertexIndexes;
            Vertexes = vertexes;
        }
    }
}
