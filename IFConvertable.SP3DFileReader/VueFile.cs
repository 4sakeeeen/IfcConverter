using IFConvertable.SP3DFileReader.DTO;

namespace IFConvertable.SP3DFileReader
{
    public sealed class VueFile
    {
        public VueFileMetadata FileInfo { get; }

        public VueFile(VueFileMetadata fileInfo)
        {
            FileInfo = fileInfo;
        }
    }
}