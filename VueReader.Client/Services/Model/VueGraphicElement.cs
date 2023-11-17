using IngrDataReadLib;
using System;
using System.Collections.Generic;
using System.Linq;

namespace IfcConverter.Client.Services.Model
{
    public sealed class VueGraphicElement
    {
        public Dictionary<string, string> Attributes { get; } = new();


        public VueGroup GeometryGroup { get; }


        public string Linkage { get; }


        public string Moniker { get; }


        public string SPFUID { get; }


        public VueGraphicElement(
            Array rawProperties,
            string linkage,
            string moniker,
            string spfuid,
            eGraphicType graphicType,
            IngrGeom geometry,
            RdMaterialProperties materialProperties)
        {
            this.Attributes = new Dictionary<string, string>(
                rawProperties
                .Cast<string>()
                .Select(delegate (string rawProperty)
                {
                    string[] propertyParts = rawProperty.Split(" : ");
                    if (propertyParts.Length != 2) { throw new Exception($"Property {rawProperty} can not be converted."); }
                    return new KeyValuePair<string, string>(propertyParts.ElementAt(0), propertyParts.ElementAt(1));
                })
                .GroupBy(delegate (KeyValuePair<string, string> x)
                {
                    return x.Key;
                })
                .Select(delegate (IGrouping<string, KeyValuePair<string, string>> x)
                {
                    return x.First();
                }));
            this.Linkage = linkage;
            this.Moniker = moniker;
            this.SPFUID = spfuid;
            this.GeometryGroup = graphicType switch
            {
                eGraphicType.GROUP_TYPE => new VueGroup((IRdGroup)geometry),
                _ => throw new NotImplementedException("Graphic type GROUP_TYPE not encountered for a convertable element."),
            };
        }
    }
}
