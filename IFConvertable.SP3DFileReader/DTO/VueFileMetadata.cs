namespace IFConvertable.SP3DFileReader.DTO
{
    public readonly struct VueFileMetadata
    {
        public string AuthorProduct { get; init; }

        public string SourceName { get; init; }

        public string ViewerProduct { get; init; }

        public string MajorVersion { get; init; }

        public string MinorVersion { get; init; }

        public Dictionary<string, string> AspectMap { get; init; }
    }
}
