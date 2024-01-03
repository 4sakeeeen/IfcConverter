namespace IFConvertable.SP3DFileReader.DTO
{
    public readonly struct TessellationOptions
    {
        private readonly int _Tolerance;

        public int Tolerance
        {
            get { return _Tolerance; }
            init
            {
                if (value < 10 || value > 30) throw new ArgumentException($"Specified tolerance option value ({value}) invalid. Accessable range is [10..30].", nameof(Tolerance));
                _Tolerance = value;
            }
        }

        public bool IncludeAll { get; init; }

        public bool IncludeBspCurve2dTypes { get; init; }

        public bool IncludeBspCurveTypes { get; init; }

        public bool IncludeBspSurfaceTypes { get; init; }

        public bool IncludeLineTypes { get; init; }

        public bool IncludeArcTypes { get; init; }

        public bool IncludeEllipseTypes { get; init; }

        public bool IncludeLineStringTypes { get; init; }

        public bool IncludeComplexStringTypes { get; init; }

        public bool IncludePlaneTypes { get; init; }

        public bool IncludeConeTypes { get; init; }

        public bool IncludeProjectionTypes { get; init; }

        public bool IncludeRevolutionTypes { get; init; }

        public bool IncludeRuledTypes { get; init; }

        public bool IncludeSphereTypes { get; init; }

        public bool IncludeTorusTypes { get; init; }

        public bool IncludePolymeshTypes { get; init; }

        public bool IncludePipeTypes { get; init; }

        public bool IncludeText3dTypes { get; init; }

        public bool IncludeLineStripTypes { get; init; }

        public bool IncludeTriangleStripTypes { get; init; }

        public bool IncludeTriangleListTypes { get; init; }

        public bool IncludeResetTesselationTypes { get; init; }
    }
}
