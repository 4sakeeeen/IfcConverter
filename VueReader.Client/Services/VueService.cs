using IfcConverter.Client.Services.Model;
using IfcConverter.Client.Services.Model.Common;
using IngrDataReadLib;
using System;
using System.Collections.Generic;

namespace IfcConverter.Client.Services
{
    public static class VueService
    {
        public static List<VueBoundary> ConvertAllBoundaries(IRdBoundary boundary)
        {
            var vueBoundaries = new List<VueBoundary>();
            boundary.GetBoundaryCount(out int boundaryCount);

            for (int i = 0; i < boundaryCount; i++)
            {
                vueBoundaries.Add(new VueBoundary(boundary, i));
            }

            return vueBoundaries;
        }
    }
}
