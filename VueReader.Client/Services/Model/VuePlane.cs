using IfcConverter.Client.Services.Model.Base;
using IfcConverter.Client.Services.Model.Common;
using System.Collections.Generic;
using System.Linq;

namespace IfcConverter.Client.Services.Model
{
    public sealed class VuePlane : VueGraphicElement
    {
        public VuePosition Normal { get; }

        public VuePosition UDirection { get; }

        public List<VueBoundary> Bounds { get; } = new();

        public VuePlane(int aspectNo, int sequenceInGroup, VuePosition normal, VuePosition uDirection, IEnumerable<VueBoundary> vueBoundaries)
            : base(aspectNo, sequenceInGroup)
        {
            this.Normal = normal;
            this.UDirection = uDirection;
            this.Bounds = vueBoundaries.ToList();
        }
    }
}
