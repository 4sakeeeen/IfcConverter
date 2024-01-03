using IFConvertable.SP3DFileReader.DTO;
using IngrDataReadLib;
using System.Text;

namespace IFConvertable.SP3DFileReader
{
    public delegate void ReadHandler(ReadResultArgs args);


    public enum ReaderActions
    {
        OPEN_FILE,
        GET_ATTRIBUTES,
        GET_NEXT_ELEMENT
    }


    public class ReadResultArgs : EventArgs
    {
        public ReaderActions Action { get; init; }

        public eErrorCode ResultCode { get; init; }

        public object? Context { get; init; }

        public ReadResultArgs(ReaderActions actionName, eErrorCode resultCode, object? context)
        {
            Action = actionName;
            ResultCode = resultCode;
            Context = context;
        }
    }


    public class VueFileReader
    {
        private readonly string _FilePath;

        private readonly IngrDataReader _Reader = new();

        public event ReadHandler? ReadPerformed;

        public VueFileReader(string filePath)
        {
            _FilePath = filePath;
            Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
        }

        public VueFileReader(string filePath, TessellationOptions options) : this(filePath)
        {
            _Reader.SetTessellatorTolerance((ushort)options.Tolerance);
            
            if (options.IncludeAll)
            {
                _Reader.SetTessellatorForType(eGraphicType.INVALID_TYPE);
            }
            else
            {
                if (options.IncludeArcTypes) _Reader.SetTessellatorForType(eGraphicType.ARC_TYPE);
                if (options.IncludeBspCurve2dTypes) _Reader.SetTessellatorForType(eGraphicType.BSPCURVE2D_TYPE);
                if (options.IncludeBspCurveTypes) _Reader.SetTessellatorForType(eGraphicType.BSPCURVE_TYPE);
                if (options.IncludeBspSurfaceTypes) _Reader.SetTessellatorForType(eGraphicType.BSPSURFACE_TYPE);
                if (options.IncludeComplexStringTypes) _Reader.SetTessellatorForType(eGraphicType.COMPLEXSTRING_TYPE);
                if (options.IncludeConeTypes) _Reader.SetTessellatorForType(eGraphicType.CONE_TYPE);
                if (options.IncludeEllipseTypes) _Reader.SetTessellatorForType(eGraphicType.ELLIPSE_TYPE);
                if (options.IncludeLineStringTypes) _Reader.SetTessellatorForType(eGraphicType.LINESTRING_TYPE);
                if (options.IncludeLineStripTypes) _Reader.SetTessellatorForType(eGraphicType.LINE_STRIP);
                if (options.IncludeLineTypes) _Reader.SetTessellatorForType(eGraphicType.LINE_TYPE);
                if (options.IncludePipeTypes) _Reader.SetTessellatorForType(eGraphicType.PIPE_TYPE);
                if (options.IncludePlaneTypes) _Reader.SetTessellatorForType(eGraphicType.PLANE_TYPE);
                if (options.IncludePolymeshTypes) _Reader.SetTessellatorForType(eGraphicType.POLYMESH_TYPE);
                if (options.IncludeProjectionTypes) _Reader.SetTessellatorForType(eGraphicType.PROJECTION_TYPE);
                if (options.IncludeResetTesselationTypes) _Reader.SetTessellatorForType(eGraphicType.RESETTESSELATION_TYPE);
                if (options.IncludeRevolutionTypes) _Reader.SetTessellatorForType(eGraphicType.REVOLUTION_TYPE);
                if (options.IncludeRuledTypes) _Reader.SetTessellatorForType(eGraphicType.RULED_TYPE);
                if (options.IncludeSphereTypes) _Reader.SetTessellatorForType(eGraphicType.SPHERE_TYPE);
                if (options.IncludeText3dTypes) _Reader.SetTessellatorForType(eGraphicType.TEXT3D_TYPE);
                if (options.IncludeTorusTypes) _Reader.SetTessellatorForType(eGraphicType.TORUS_TYPE);
                if (options.IncludeTriangleListTypes) _Reader.SetTessellatorForType(eGraphicType.TRIANGLE_LIST);
                if (options.IncludeTriangleStripTypes) _Reader.SetTessellatorForType(eGraphicType.TRIANGLE_STRIP);
            }
        }


        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        /// <exception cref="Exception"></exception>
        public VueHierarchy ReadHierarchy()
        {
            _Reader.OpenVueFile(_FilePath, out eErrorCode errCode);
            ReadPerformed?.Invoke(new ReadResultArgs(ReaderActions.OPEN_FILE, errCode, null));

            if (errCode != eErrorCode.E_OK) throw new Exception();

            VueHierarchy hierarchy = new();

            while (errCode != eErrorCode.E_EOF)
            {
                _Reader.GetCurrentGraphicIdentifier(out string linkage, out string moniker, out string spfuid);
                
                _Reader.GetCurrentGraphicLabelProperties(out Array properties, out errCode);
                Dictionary<string, string> attributes = ParseAttributes(properties);
                ReadPerformed?.Invoke(new ReadResultArgs(ReaderActions.GET_ATTRIBUTES, errCode, attributes));
                
                _Reader.GetNextGraphicElement(out errCode);
                ReadPerformed?.Invoke(new ReadResultArgs(ReaderActions.GET_NEXT_ELEMENT, errCode, null));

                attributes.TryGetValue("Name", out string? name);
                attributes.TryGetValue("System Path", out string? systemPath);

                SmartId smartId = ParseMoniker(moniker);

                hierarchy.Insert(smartId, name ?? "NO_NAME", systemPath != null ? systemPath.Split('\\') : Array.Empty<string>());
            }

            _Reader.CloseVueFile();

            return hierarchy;
        }

        public VueFileMetadata ReadFileMetadata()
        {
            _Reader.OpenVueFile(_FilePath, out eErrorCode errCode);
            ReadPerformed?.Invoke(new ReadResultArgs(ReaderActions.OPEN_FILE, errCode, _FilePath));
            SourceFileInfo info = new();
            _Reader.GetSourceFileInfo(ref info);
            _Reader.GetAspectMap(out Array aspectNumbers, out Array aspectNames);
            _Reader.CloseVueFile();

            return new VueFileMetadata
            {
                AuthorProduct = info.AuthorProduct,
                SourceName = info.SorceFileName,
                ViewerProduct = info.ViewerProduct,
                MajorVersion = info.FileMajorVersion,
                MinorVersion = info.FileMinorVersion,
                AspectMap = new Dictionary<string, string>(aspectNumbers.Length)
            };
        }

        private Dictionary<string, string> ParseAttributes(Array attributes)
        {
            return new Dictionary<string, string>(attributes
                .Cast<string>().Select(x =>
                {
                    string[] parts = x.Split(" : ");
                    return new KeyValuePair<string, string>(parts.ElementAt(0), Encoding.GetEncoding(1251).GetString(Encoding.GetEncoding(1252).GetBytes(string.Concat(parts.Skip(1)))));
                })
                .GroupBy(x => x.Key).Select(x => x.First()));
        }

        private SmartId ParseMoniker(string moniker)
        {
            return new SmartId
            {
                ClassID = int.Parse(moniker.Split("!!")[1].Split("##")[0]),
                UID = moniker.Split("##")[1]
            };
        }
    }
}
