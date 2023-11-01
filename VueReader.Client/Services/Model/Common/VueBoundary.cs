using IfcConverter.Client.Services.Model.Base;
using System.Collections.Generic;

namespace IfcConverter.Client.Services.Model.Common
{
    public sealed class VueBoundary
    {
        public List<VueBoundaryElement> Curves { get; } = new();
    }
}
