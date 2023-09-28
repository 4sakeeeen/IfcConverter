namespace VueReader.Domain.Models.BuiltIn
{
    /// <summary>
    /// The SourceFileInfo structure represents the input vue/zvf files source information.
    /// </summary>
    public struct SourceFileInfo
    {
        /// <summary>
        /// // Authoring product information (Smart 3D/Smart Plant Review).
        /// </summary>
        public string AuthorProduct;
        public string SourceFileName;
        /// <summary>
        /// File viewer product (Smart Plant Review).
        /// </summary>
        public string ViewerProduct;
        public string FileMajorVersion;
        public string FileMinorVersion;

        public SourceFileInfo(string authorProduct, string sourceFileName, string viewerProduct, string fileMajorVersion, string fileMinorVersion)
        {
            AuthorProduct = authorProduct;
            SourceFileName = sourceFileName;
            ViewerProduct = viewerProduct;
            FileMajorVersion = fileMajorVersion;
            FileMinorVersion = fileMinorVersion;
        }
    }
}
