namespace VueReader.Domain.Models.BuiltIn
{
    /// <summary>
    /// The TriStrip structure contains triangle strip data information. The m_iVertexCount parameter represents the vertex count.The vertex data is represented by m_pVertex.
    /// The face count (number of triangles) is represented by m_iTriangleCount.The m_pVertexIndex represents the vertex index, which is the arrangement of the vertices to create a meaningful
    /// face.The three values represent a triangle.For example: (0 2 3) (3 2 1) (3 4 6) (………….. The m_pVertexNormal represents the array of normal data on the vertices.The m_pTextureCord parameter represents the 2D texture data on the vertices.
    /// </summary>
    public struct TriStrip
    {
        public long VertexCount;
        public long TriangleCount;
        public long[] VertexIndexes;
        public Position[] Vertexes;
        public Position[] VertexNormals;
        public Position2d[] TextureCords;

        public TriStrip(long vertexCount, long triangleCount, long[] vertexIndexes, Position[] vertexes, Position[] vertexNormals, Position2d[] textureCords)
        {
            VertexCount = vertexCount;
            TriangleCount = triangleCount;
            VertexIndexes = vertexIndexes;
            Vertexes = vertexes;
            VertexNormals = vertexNormals;
            TextureCords = textureCords;
        }
    }
}
