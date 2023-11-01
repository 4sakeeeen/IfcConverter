using IfcConverter.Client.Services.Model.Base;
using System.Collections.Generic;

namespace IfcConverter.Client.Services.Model
{
    public sealed class VueGroup
    {
        public List<VueGraphicElement> GraphicElements { get; } = new();
    }
}
